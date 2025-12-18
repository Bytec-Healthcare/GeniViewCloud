using GeniView.Cloud.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

namespace GeniView.Cloud.Controllers.API
{
    public class BaseApiController : ApiController
    {
        public GeniViewCloudDataRepository _db = new GeniViewCloudDataRepository();

        protected long GetDefaultAgentID()
        {
            var findAgent = _db.Agents.Where(a => a.Name.ToLower() == "default").FirstOrDefault();
            if (findAgent != null)
            {
                return findAgent.ID;
            }
            else
            {
                return -1;
            }
        }

        protected ResponseMessageResult ResponseErrorMessage(HttpStatusCode httpStatusCode, string errorMessage)
        {
            var jObject = new JObject
            {
                { "Message", errorMessage }
            };

            var response = Request.CreateResponse(httpStatusCode);
            response.Content = new StringContent(
                    JsonConvert.SerializeObject(jObject),
                    Encoding.UTF8,
                    "application/json");
            return ResponseMessage(response);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.Collect();
            }
            base.Dispose(disposing);
        }

    }


}
