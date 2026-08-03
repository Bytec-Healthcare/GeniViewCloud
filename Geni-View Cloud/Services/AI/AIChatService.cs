using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GeniView.Cloud.Services.AI
{
    /// <summary>
    /// Orchestrates the three POC scenarios:
    ///   GET_BATTERY_PERFORMANCE  — "How is battery B100 performing?"
    ///   COMPARE_BATTERIES        — "Compare battery B100 and B200."
    ///   OVERALL_BATTERY_SUMMARY  — "Give me an overall battery summary."
    /// </summary>
    public class AIChatService
    {
        private readonly ILLMService _llm;
        private readonly BatteriesDataRepository _batteryRepo;
        private readonly GeniViewCloudDataRepository _db;
        private readonly ILogger<AIChatService> _logger;

        private const string SystemPrompt =
            "You are the Geniview Cloud Battery Assistant. " +
            "You help operational users understand the health and performance of their batteries. " +
            "Always respond in a concise, professional, and friendly tone. " +
            "Base your response ONLY on the data provided to you. " +
            "Do NOT invent metrics, values, or predictions. " +
            "If a metric is unavailable, say it is not currently available. " +
            "Do NOT mention that you are an AI, and do NOT reference the raw data format.";

        public AIChatService(
            ILLMService llm,
            BatteriesDataRepository batteryRepo,
            GeniViewCloudDataRepository db,
            ILogger<AIChatService> logger)
        {
            _llm         = llm;
            _batteryRepo = batteryRepo;
            _db          = db;
            _logger      = logger;
        }

        // ── Public entry point ────────────────────────────────────────────────

        public async Task<string> HandleMessageAsync(string userMessage, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Please enter a question about your batteries.";

            _logger.LogInformation("AI chat request: {Message}", userMessage);

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Step 1 — detect intent via LLM
            IntentResult intent;
            try
            {
                intent = await DetectIntentAsync(userMessage, ct);
                _logger.LogInformation("Detected intent: {Intent} | Batteries: [{Ids}]",
                    intent.Intent, string.Join(", ", intent.BatteryIds ?? new List<string>()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Intent detection failed.");
                return "I had trouble understanding your question. Could you rephrase it? " +
                       "Try: \"How is battery B100 performing?\", " +
                       "\"Compare battery B100 and B200\", or " +
                       "\"Give me an overall battery summary.\"";
            }

            if (intent.Intent == "UNKNOWN")
            {
                _logger.LogInformation("Intent detection returned UNKNOWN, using fallback response.");
            }

            string answer;
            try
            {
                answer = intent.Intent switch
                {
                    "GET_BATTERY_PERFORMANCE"  => await HandleBatteryPerformanceAsync(intent, userMessage, ct),
                    "COMPARE_BATTERIES"        => await HandleCompareBatteriesAsync(intent, userMessage, ct),
                    "OVERALL_BATTERY_SUMMARY"  => await HandleOverallSummaryAsync(userMessage, ct),
                    _                          => FallbackResponse()
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error handling intent {Intent}.", intent.Intent);
                return "I encountered an error while retrieving battery data. Please try again.";
            }

            sw.Stop();
            _logger.LogInformation("AI chat completed in {ElapsedMs}ms.", sw.ElapsedMilliseconds);
            return answer;
        }

        // ── Intent detection ─────────────────────────────────────────────────

        private async Task<IntentResult> DetectIntentAsync(string userMessage, CancellationToken ct)
        {
            var intentSystemPrompt =
                "You extract structured intent from battery-related queries. " +
                "Respond ONLY with a valid JSON object in this exact format (no markdown, no explanation):\n" +
                "{\"intent\": \"GET_BATTERY_PERFORMANCE\", \"batteryIds\": [\"B100\"]}\n\n" +
                "Valid intents:\n" +
                "  GET_BATTERY_PERFORMANCE  — user asks about ONE battery's status or performance.\n" +
                "  COMPARE_BATTERIES        — user asks to compare TWO or more batteries.\n" +
                "  OVERALL_BATTERY_SUMMARY  — user wants a fleet-wide summary or overview.\n" +
                "  UNKNOWN                  — does not fit any of the above.\n\n" +
                "For batteryIds, extract the serial number(s) mentioned (e.g. B100, GV-B100, 2156593322). " +
                "If none are mentioned, return an empty array []. " +
                "Return ONLY the JSON object.";

            var rawJson = await _llm.GenerateResponseAsync(intentSystemPrompt, userMessage, ct);
            _logger.LogInformation("Intent detection raw Gemini response: {Response}", rawJson);

            // Strip markdown code fences if present
            var cleanedJson = rawJson.Trim();
            if (cleanedJson.StartsWith("```"))
            {
                var start = cleanedJson.IndexOf('{');
                var end   = cleanedJson.LastIndexOf('}');
                if (start >= 0 && end > start)
                    cleanedJson = cleanedJson.Substring(start, end - start + 1);
            }
            _logger.LogInformation("Intent detection cleaned JSON: {CleanedResponse}", cleanedJson);

            try
            {
                var result = JsonSerializer.Deserialize<IntentResult>(cleanedJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new IntentResult { Intent = "UNKNOWN" };
            }
            catch
            {
                _logger.LogWarning("Intent detection failed to parse Gemini response as JSON.");
                return new IntentResult { Intent = "UNKNOWN" };
            }
        }

        // ── Scenario 1 — Single battery performance ──────────────────────────

        private async Task<string> HandleBatteryPerformanceAsync(
            IntentResult intent, string userMessage, CancellationToken ct)
        {
            var batteryId = intent.BatteryIds?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(batteryId))
                return "Which battery would you like to know about? Please mention its serial number, for example: \"How is battery B100 performing?\"";

            var detail = FindBatteryBySerialNumber(batteryId);
            if (detail == null)
                return $"I couldn't find a battery with serial number \"{batteryId}\". " +
                       "Please check the serial number and try again.";

            var dataContext = BuildSingleBatteryContext(detail);

            var userPrompt =
                $"Battery data:\n{dataContext}\n\n" +
                $"User question: {userMessage}\n\n" +
                "Provide a concise, human-friendly operational summary. " +
                "Include: online/offline status, state of charge, temperature, current status, " +
                "and any noteworthy observations. Keep it under 150 words.";

            return await _llm.GenerateResponseAsync(SystemPrompt, userPrompt, ct);
        }

        // ── Scenario 2 — Compare two batteries ───────────────────────────────

        private async Task<string> HandleCompareBatteriesAsync(
            IntentResult intent, string userMessage, CancellationToken ct)
        {
            var ids = intent.BatteryIds ?? new List<string>();
            if (ids.Count < 2)
                return "To compare batteries I need at least two serial numbers. " +
                       "For example: \"Compare battery B100 and B200.\"";

            var details = ids.Take(2)
                             .Select(id => new { Id = id, Detail = FindBatteryBySerialNumber(id) })
                             .ToList();

            var notFound = details.Where(d => d.Detail == null).Select(d => d.Id).ToList();
            if (notFound.Any())
                return $"I couldn't find the following batteries: {string.Join(", ", notFound.Select(id => $"\"{id}\""))}. " +
                       "Please check the serial numbers and try again.";

            var sb = new StringBuilder();
            foreach (var d in details)
            {
                sb.AppendLine($"--- Battery {d.Id} ---");
                sb.AppendLine(BuildSingleBatteryContext(d.Detail));
                sb.AppendLine();
            }

            var userPrompt =
                $"Battery comparison data:\n{sb}\n\n" +
                $"User question: {userMessage}\n\n" +
                "Compare these batteries side by side. Cover: state of charge, temperature, " +
                "operating status, and overall condition. Clearly state which battery appears " +
                "to be in better condition and why. Keep it under 200 words.";

            return await _llm.GenerateResponseAsync(SystemPrompt, userPrompt, ct);
        }

        // ── Scenario 3 — Overall fleet summary ───────────────────────────────

        private async Task<string> HandleOverallSummaryAsync(string userMessage, CancellationToken ct)
        {
            var allBatteries = _batteryRepo.GetBatteries(null, null, true).ToList();

            if (!allBatteries.Any())
                return "There are currently no batteries registered in the system.";

            var offlineThreshold = GlobalSettings.OnlineRangeInMinutes; // already a DateTime threshold

            int total      = allBatteries.Count;
            int online     = allBatteries.Count(b => b.isOnline);
            int offline    = total - online;

            // Aggregate from batteries that have recent telemetry
            var withLogs = allBatteries
                .Where(b => b.LastAgentBatteryLog != null)
                .ToList();

            double? avgSoc  = withLogs.Any()
                ? Math.Round(withLogs.Average(b =>
                    b.LastAgentBatteryLog.SlowChangingDataA?.RelativeStateOfCharge ?? 0), 1)
                : (double?)null;

            double? avgTemp = withLogs.Any()
                ? Math.Round(withLogs.Average(b =>
                    b.LastAgentBatteryLog.SlowChangingDataB?.BatteryInternalTemperature ?? 0), 1)
                : (double?)null;

            int lowCharge  = withLogs.Count(b =>
                (b.LastAgentBatteryLog.SlowChangingDataA?.RelativeStateOfCharge ?? 100)
                < GlobalSettings.AlertChargingLVL);

            int highTemp   = withLogs.Count(b =>
                (b.LastAgentBatteryLog.SlowChangingDataB?.BatteryInternalTemperature ?? 0)
                > GlobalSettings.AlertTemperature);

            int charging    = withLogs.Count(b => b.LastAgentBatteryLog.Status == BatteryStates.Charging);
            int discharging = withLogs.Count(b => b.LastAgentBatteryLog.Status == BatteryStates.PoweringSystem);
            int idle        = withLogs.Count(b => b.LastAgentBatteryLog.Status == BatteryStates.Idle);

            var summary = new StringBuilder();
            summary.AppendLine($"Total batteries:      {total}");
            summary.AppendLine($"Online batteries:     {online}");
            summary.AppendLine($"Offline batteries:    {offline}");
            summary.AppendLine($"Currently charging:   {charging}");
            summary.AppendLine($"Currently discharging:{discharging}");
            summary.AppendLine($"Currently idle:       {idle}");
            if (avgSoc.HasValue)
                summary.AppendLine($"Average state of charge: {avgSoc}%");
            if (avgTemp.HasValue)
                summary.AppendLine($"Average temperature: {avgTemp}°C");
            summary.AppendLine($"Batteries with critically low charge (<{GlobalSettings.AlertChargingLVL}%): {lowCharge}");
            summary.AppendLine($"Batteries with high temperature (>{GlobalSettings.AlertTemperature}°C): {highTemp}");

            var userPrompt =
                $"Fleet summary data:\n{summary}\n\n" +
                $"User question: {userMessage}\n\n" +
                "Convert this into a concise, management-friendly fleet health summary. " +
                "Highlight any areas of concern. Keep it under 180 words.";

            return await _llm.GenerateResponseAsync(SystemPrompt, userPrompt, ct);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Tries to find a battery by serial number string (flexible matching).
        /// Looks up BatteriesListViewModel (which includes the latest telemetry).
        /// </summary>
        private BatteriesListViewModelForAI FindBatteryBySerialNumber(string serialId)
        {
            var all = _batteryRepo.GetBatteries(null, null, true).ToList();

            // Normalise: strip common prefixes for matching
            var normalised = serialId.Trim().ToUpperInvariant();

            var match = all.FirstOrDefault(b =>
            {
                var sn = (b.Battery?.SerialNumber ?? "").ToUpperInvariant();
                var snCode = b.Battery?.SerialNumberCode?.ToString() ?? "";
                return sn == normalised
                    || snCode == serialId.Trim()
                    || sn.EndsWith(normalised)
                    || normalised.EndsWith(sn);
            });

            if (match == null) return null;

            return new BatteriesListViewModelForAI
            {
                SerialNumber  = match.Battery?.SerialNumber ?? serialId,
                IsOnline      = match.isOnline,
                Status        = match.Status?.Name ?? "Unknown",
                LastSeenOn    = match.LastSeenOn,
                SOC           = match.LastAgentBatteryLog?.SlowChangingDataA?.RelativeStateOfCharge,
                Temperature   = match.LastAgentBatteryLog?.SlowChangingDataB?.BatteryInternalTemperature,
                Voltage       = match.LastAgentBatteryLog != null
                                    ? Math.Round(match.LastAgentBatteryLog.OperatingData?.Voltage ?? 0, 2)
                                    : (double?)null,
                Current       = match.LastAgentBatteryLog != null
                                    ? Math.Round(match.LastAgentBatteryLog.OperatingData?.Current ?? 0, 2)
                                    : (double?)null,
                RemainingCapacity = match.LastAgentBatteryLog?.SlowChangingDataA?.RemainingCapacity,
                ChargingLevel = match.ChargingLevel?.Name,
                AlertStatus   = match.Alert?.Name,
                Community     = match.Community?.Name,
                Group         = match.Group?.Name,
                BatteryStates = match.LastAgentBatteryLog?.Status.ToString()
            };
        }

        private static string BuildSingleBatteryContext(BatteriesListViewModelForAI b)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Serial Number:        {b.SerialNumber}");
            sb.AppendLine($"Online status:        {(b.IsOnline ? "Online" : "Offline")}");
            sb.AppendLine($"Operating status:     {b.Status}");
            sb.AppendLine($"Battery state:        {b.BatteryStates ?? "Unknown"}");
            if (b.SOC.HasValue)
                sb.AppendLine($"State of charge:      {b.SOC}%");
            if (b.Temperature.HasValue)
                sb.AppendLine($"Internal temperature: {b.Temperature}°C");
            if (b.Voltage.HasValue)
                sb.AppendLine($"Voltage:              {b.Voltage}V");
            if (b.Current.HasValue)
                sb.AppendLine($"Current:              {b.Current}A");
            if (b.RemainingCapacity.HasValue)
                sb.AppendLine($"Remaining capacity:   {b.RemainingCapacity:F2} Ah");
            if (b.ChargingLevel != null)
                sb.AppendLine($"Charging level:       {b.ChargingLevel}%");
            if (b.AlertStatus != null)
                sb.AppendLine($"Alert status:         {b.AlertStatus}");
            if (b.LastSeenOn.HasValue)
                sb.AppendLine($"Last communication:   {b.LastSeenOn:yyyy-MM-dd HH:mm} UTC");
            if (b.Community != null)
                sb.AppendLine($"Community:            {b.Community}");
            if (b.Group != null)
                sb.AppendLine($"Group:                {b.Group}");
            return sb.ToString();
        }

        private static string FallbackResponse() =>
            "I can help with three types of battery questions:\n" +
            "• \"How is battery [serial number] performing?\"\n" +
            "• \"Compare battery [serial 1] and [serial 2]\"\n" +
            "• \"Give me an overall battery summary\"";

        // ── Inner types ───────────────────────────────────────────────────────

        private class IntentResult
        {
            public string Intent { get; set; } = "UNKNOWN";
            public List<string> BatteryIds { get; set; } = new();
        }

        /// <summary>Flat AI-safe DTO — only send what Gemini needs.</summary>
        private class BatteriesListViewModelForAI
        {
            public string    SerialNumber      { get; set; }
            public bool      IsOnline          { get; set; }
            public string    Status            { get; set; }
            public string    BatteryStates     { get; set; }
            public int?      SOC               { get; set; }
            public int?      Temperature       { get; set; }
            public double?   Voltage           { get; set; }
            public double?   Current           { get; set; }
            public double?   RemainingCapacity { get; set; }
            public string    ChargingLevel     { get; set; }
            public string    AlertStatus       { get; set; }
            public DateTime? LastSeenOn        { get; set; }
            public string    Community         { get; set; }
            public string    Group             { get; set; }
        }
    }
}
