using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GeniView.Cloud.Controllers.API
{
    public class CommandApiController : BaseApiController
    {
        public CommandApiController(GeniViewCloudDataRepository db, BatteriesDataRepository batteriesRepo)
            : base(db)
        {
            _batteriesrpo = batteriesRepo;
        }


        public class CmdRequest
        {
            public string Type { get; set; }
            public string Data { get; set; }
            public string DeviceType { get; set; }
            public string DeviceId { get; set; }
        }
        public enum CMDCATEGORY
        {
            BATTERY,
            DOCK,
            BATTERYRESULT,
            DOCKRESULT
        }

        public Dictionary<string, string> BasicPublishTopics { get; set; } = new Dictionary<string, string>()
        {
            { CMDCATEGORY.BATTERY.ToString(), "battery/cmd/" },
            { CMDCATEGORY.DOCK.ToString()   , "dock/cmd/"},
            { CMDCATEGORY.BATTERYRESULT.ToString()   , "server/cmd/battery/result/"},
            { CMDCATEGORY.DOCKRESULT.ToString()   , "server/cmd/dock/result/"}
        };

        private readonly BatteriesDataRepository _batteriesrpo;
        private static Logger _logger = LogManager.GetCurrentClassLogger();


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/SendCommand/")]
        public IActionResult SendCommand(CmdRequest cmd)
        {
            try
            {
                //According to the command type, publish the message to MQTT broker
                if (BasicPublishTopics.ContainsKey(cmd.DeviceType.ToUpper()) == false)
                {
                    return BadRequest("Device type is not exist!");
                }
                else
                {
                    //Prepare the topic for specify usage
                    string topic = $"{BasicPublishTopics[cmd.DeviceType.ToUpper()]}{cmd.DeviceId}";

                    //Transfer the data to real command
                    string message = cmd.Data;

                    //Publish to deivce
                    //MQTTHelper.Instance.Publish(topic, message, MqttQualityOfServiceLevel.ExactlyOnce);

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/ReportFrequencyBatch/")]
        public async Task<IActionResult> GetReportFrequency([FromBody] List<string> SerialNumberCode )
        {
            var result = "";
            try
            {
                if (SerialNumberCode == null || SerialNumberCode.Any() == false)
                {
                    SerialNumberCode = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode.ToString()).ToList();
                }
                else
                {
                    var ret = SerialNumberCode.Where(x => 
                        !_batteriesrpo.GetBatteries(_db).Select(b => b.SerialNumberCode.ToString()).Contains(x)
                    ).ToList();

                    StringBuilder sb = new StringBuilder();

                    foreach (var item in ret)
                    {
                        sb.Append($"{item},");
                    }

                    if (ret != null && ret.Any() == true)
                    {
                        return ResponseErrorMessage(HttpStatusCode.NotFound, sb.ToString().TrimEnd(','));
                    }
                }
               

                var data = Global._memCacheHelper.GetLogRateResult(SerialNumberCode);

                return Ok(data);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/ReportFrequency/")]
        public async Task<IActionResult> PostReportFrequency([FromForm] LogRate logRate, [FromQuery] string SerialNumberCode = null)
        {
            List<object> result = new List<object>();

            try
            {
                // Validate required fields manually (skip ModelState — it has false positives from model binding)
                if (string.IsNullOrEmpty(SerialNumberCode))
                {
                    return BadRequest("SerialNumberCode is required. Please select a battery.");
                }

                if (logRate == null || logRate.IntervalSec < 1)
                {
                    return BadRequest("IntervalSec is required and must be at least 1.");
                }

                long snCodeValue;
                if (!long.TryParse(SerialNumberCode, out snCodeValue))
                {
                    return BadRequest($"SerialNumberCode '{SerialNumberCode}' is not a valid number.");
                }

                var exist = _batteriesrpo
                    .GetBatteries(_db).Where(x => x.SerialNumberCode == snCodeValue).Any();

                if (exist == true)
                {
                    if (!MQTTHelper.Instance.IsConnected)
                    {
                        return StatusCode(503, "MQTT broker is not connected. Please check the broker and try again.");
                    }

                    string topic = MQTTTopic.GetLogRate(SerialNumberCode.ToString());

                    LogRate cmd = new LogRate(SerialNumberCode.ToString(), logRate.IntervalSec);
                    string para = JsonConvert.SerializeObject(cmd);

                    var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.ExactlyOnce);
                    var data = new { SN = SerialNumberCode, ret.IsSuccess, ret.ReasonCode, ret.ReasonString };
                    result.Add(data);

                    return Ok(result);
                }
                else
                {
                    return BadRequest($"SerialNumberCode {SerialNumberCode} doesn't exist.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                var errorMsg = !string.IsNullOrEmpty(ex.Message) 
                    ? ex.Message 
                    : "An unexpected error occurred. Check if MQTT broker is connected.";
                return StatusCode(500, errorMsg);
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/SetOTA/")]
        public async Task<IActionResult> PostOTA([FromQuery] string SerialNumberCode = null)
        {
            List<object> result = new List<object>();

            string filePath = Path.Combine(Global._serverPath, Global._otaPath);

            _logger.Info($"SetOTA: checking file at '{filePath}'");

            if (System.IO.File.Exists(filePath) == false)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, $"OTA file not found at: {filePath}");
            }

            if (!MQTTHelper.Instance.IsConnected)
            {
                return ResponseErrorMessage(HttpStatusCode.ServiceUnavailable, "MQTT broker is not connected. Please check the broker and try again.");
            }

            try
            {
                if (ModelState.IsValid == true)
                {
                    string otaRelPath = Global._otaPath.Replace('\\', '/');
                string path = $"{Request.Scheme}://{Request.Host}/{otaRelPath}";

                    if (string.IsNullOrEmpty(SerialNumberCode) == true)
                    {
                        return BadRequest("SerialNumberCode is required. Please select a battery.");
                    }
                    else
                    {
                        //Specify a device
                        long snCodeValue;
                        if (!long.TryParse(SerialNumberCode, out snCodeValue))
                        {
                            return BadRequest($"SerialNumberCode '{SerialNumberCode}' is not a valid number.");
                        }

                        var exist = _batteriesrpo
                            .GetBatteries(_db).Where(x => x.SerialNumberCode == snCodeValue).Any();
                        OTA cmd = new OTA(SerialNumberCode, path);


                        if (exist == true)
                        {
                            // Step 1: OTA Started
                            CustomMessage startMsg = new CustomMessage(SerialNumberCode, "OTA Started");
                            await MQTTHelper.Instance.PublishAsync(MQTTTopic.GetCustomMessage(SerialNumberCode), JsonConvert.SerializeObject(startMsg), MqttQualityOfServiceLevel.ExactlyOnce);

                            // Step 2: OTA bin — retained=true so offline battery gets it on reconnect.
                            // Broker clears the retain once handleOTAResult receives Result=true.
                            string para = JsonConvert.SerializeObject(cmd);
                            string topic = MQTTTopic.GetOTA(SerialNumberCode.ToString());

                            var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.AtLeastOnce, retain: true);
                            var data = new { SN = SerialNumberCode, ret.IsSuccess, ret.ReasonCode, ret.ReasonString };
                            result.Add(data);

                            // Step 3: OTA Done
                            CustomMessage doneMsg = new CustomMessage(SerialNumberCode, "OTA Done");
                            await MQTTHelper.Instance.PublishAsync(MQTTTopic.GetCustomMessage(SerialNumberCode), JsonConvert.SerializeObject(doneMsg), MqttQualityOfServiceLevel.ExactlyOnce);

                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest($"SerialNumberCode {SerialNumberCode} doesn't exist.");
                        }
                    }
                }
                else
                {
                    var message = string.Join(" ", ModelState.Values
                         .SelectMany(v => v.Errors)
                         .Select(e => e.ErrorMessage));

                    return BadRequest(message);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/GetOTA/")]
        public async Task<IActionResult> GetOTA([FromBody] List<string> SerialNumberCode)
        {
            var result = "";
            try
            {
                if (SerialNumberCode == null || SerialNumberCode.Any() == false)
                {
                    SerialNumberCode = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode.ToString()).ToList();
                }
                else
                {
                    var ret = SerialNumberCode.Where(x =>
                        !_batteriesrpo.GetBatteries(_db).Select(b => b.SerialNumberCode.ToString()).Contains(x)
                    ).ToList();


                    if (ret != null && ret.Any() == true)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (var item in ret)
                        {
                            sb.Append($"{item},");
                        }

                        return ResponseErrorMessage(HttpStatusCode.NotFound, sb.ToString().TrimEnd(','));
                    }
                }


                var data = Global._memCacheHelper.GetOTAResult(SerialNumberCode);

                return Ok(data);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/UploadOTAFile/")]
        public async Task<IActionResult> UploadOTAFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return StatusCode((int)System.Net.HttpStatusCode.UnsupportedMediaType, "No file uploaded.");
            }

            try
            {
                string filePath = Path.Combine(Global._serverPath, Global._otaPath);
                string fileUrlPath = $"{Request.Scheme}://{Request.Host}/{Global._otaPath.Replace('\\', '/')}";

                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                }

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(fs);
                }

                return Ok(fileUrlPath);
            }
            catch (Exception ex)
            {
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/SetNTP/")]
        public async Task<IActionResult> PostNTP([FromForm] NTP ntp, [FromQuery] string SerialNumberCode = null)
        {
            List<object> result = new List<object>();

            try
            {
                // Remove any model state errors for SerialNumberCode (it's not part of NTP model)
                ModelState.Remove("SerialNumberCode");

                if (ModelState.IsValid == true)
                {

                    if (string.IsNullOrEmpty(ntp.NTPURL) == true && string.IsNullOrEmpty(ntp.NTPUTC) == true)
                    {
                        return BadRequest("NTP URL or UTC is empty.");
                    }

                    if (ntp.NTPURL != null && ntp.NTPURL.Length >= 1)
                    {
                        ntp.NTPUTC = "";
                    }
                    else
                    {
                        ntp.NTPURL = "";
                    }

                    if (string.IsNullOrEmpty(SerialNumberCode) == true)
                    {
                        return BadRequest("SerialNumberCode is required. Please select a battery.");
                    }
                    else
                    {
                        //Specify a device
                        long snCodeValue;
                        if (!long.TryParse(SerialNumberCode, out snCodeValue))
                        {
                            return BadRequest($"SerialNumberCode '{SerialNumberCode}' is not a valid number.");
                        }

                        var exist = _batteriesrpo
                            .GetBatteries(_db).Where(x => x.SerialNumberCode == snCodeValue).Any();

                        NTP cmd = new NTP(SerialNumberCode, ntp.NTPURL, ntp.NTPUTC);


                        if (exist == true)
                        {
                            if (!MQTTHelper.Instance.IsConnected)
                            {
                                return ResponseErrorMessage(HttpStatusCode.ServiceUnavailable, "MQTT broker is not connected. Please check the broker and try again.");
                            }

                            string para = JsonConvert.SerializeObject(cmd);
                            string topic = MQTTTopic.GetNTP(SerialNumberCode.ToString());

                            var ret = await MQTTHelper.Instance.PublishAsync(topic, para, MqttQualityOfServiceLevel.ExactlyOnce);
                            var data = new { SN = SerialNumberCode, ret.IsSuccess, ret.ReasonCode, ret.ReasonString };
                            result.Add(data);

                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest($"SerialNumberCode {SerialNumberCode} doesn't exist.");
                        }
                    }
                }
                else
                {
                    var message = string.Join(" ", ModelState.Values
                         .SelectMany(v => v.Errors)
                         .Select(e => e.Exception));

                    var ret = CollectModelState(ModelState);
                    return BadRequest(ret);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("api/Command/GetNTP/")]
        public async Task<IActionResult> GetNTP([FromBody] List<string> SerialNumberCode)
        {
            var result = "";
            try
            {
                if (SerialNumberCode == null || SerialNumberCode.Any() == false)
                {
                    SerialNumberCode = _batteriesrpo.GetBatteries(_db).Select(x => x.SerialNumberCode.ToString()).ToList();
                }
                else
                {
                    var ret = SerialNumberCode.Where(x =>
                        !_batteriesrpo.GetBatteries(_db).Select(b => b.SerialNumberCode.ToString()).Contains(x)
                    ).ToList();


                    if (ret != null && ret.Any() == true)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (var item in ret)
                        {
                            sb.Append($"{item},");
                        }

                        return ResponseErrorMessage(HttpStatusCode.NotFound, sb.ToString().TrimEnd(','));
                    }
                }


                var data = Global._memCacheHelper.GetNTPResult(SerialNumberCode);

                return Ok(data);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        private string CollectModelState(ModelStateDictionary ModelState)
        {
            var errorMessageBuilder = new StringBuilder();

            foreach (var key in ModelState.Keys)
            {
                var modelStateEntry = ModelState[key];
                if (modelStateEntry.Errors.Any())
                {
                    errorMessageBuilder.AppendLine($"Key: {key}");
                    foreach (var error in modelStateEntry.Errors)
                    {
                        errorMessageBuilder.AppendLine($"  ErrorMessage: {error.ErrorMessage}");
                        if (error.Exception != null)
                        {
                            errorMessageBuilder.AppendLine($"  Exception: {error.Exception.Message}");
                        }
                    }
                }
            }

            return errorMessageBuilder.ToString();
        }

        /// <summary>
        /// Returns batteries filtered by community and/or group for the Command page header dropdown.
        /// GET /api/Command/GetBatteriesByFilter?communityId=1&groupId=2
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet, Route("api/Command/GetBatteriesByFilter/")]
        public IActionResult GetBatteriesByFilter(long? communityId = null, long? groupId = null)
        {
            try
            {
                IEnumerable<BatteriesListViewModel> batteries;

                if (groupId.HasValue)
                {
                    // Group selected — exact match only, no child groups
                    // (includeAllSubGroups=false so Ward 1/Ward 2 children are excluded when Bytec Stand is selected)
                    batteries = _batteriesrpo.GetBatteries(communityId, groupId, includeAllSubGroups: false);
                }
                else if (communityId.HasValue)
                {
                    // Community only — return all batteries in that community
                    batteries = _batteriesrpo.GetBatteries(communityId, null, includeAllSubGroups: false);
                }
                else
                {
                    // No filter — return all batteries
                    batteries = _batteriesrpo.GetBatteries(null, null, includeAllSubGroups: false);
                }

                var result = batteries
                    .Select(b => new
                    {
                        id                 = b.ID,
                        serialNumber       = b.Battery != null
                            ? (b.Battery.SerialNumber ?? b.Battery.SerialNumberCode?.ToString() ?? b.ID.ToString())
                            : b.ID.ToString(),
                        serialNumberCode   = b.Battery?.SerialNumberCode?.ToString() ?? "",
                        isOnline           = b.isOnline
                    })
                    .OrderBy(b => b.serialNumber)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return ResponseErrorMessage(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}
