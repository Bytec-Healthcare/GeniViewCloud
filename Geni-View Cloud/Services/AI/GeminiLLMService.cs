using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GeniView.Cloud.Services.AI
{
    /// <summary>
    /// Calls the Google Gemini REST API (generateContent endpoint).
    /// Replace this class with AzureOpenAILLMService or OpenAILLMService to switch providers.
    /// </summary>
    public class GeminiLLMService : ILLMService
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _endpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GeminiLLMService> _logger;

        // Default model — override via AIChatbot:GeminiModel in appsettings.
        // gemini-1.5-flash is deprecated. gemini-2.5-pro is the current recommended
        // price/performance model that supports system_instruction + generateContent.
        private const string DefaultModel    = "gemini-2.5-pro";
        private const string GeminiBaseUrl   = "https://generativelanguage.googleapis.com/v1beta/models/";

        public GeminiLLMService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<GeminiLLMService> logger)
        {
            _apiKey           = configuration["AIChatbot:GeminiApiKey"]
                                ?? throw new InvalidOperationException(
                                    "AIChatbot:GeminiApiKey is not configured. " +
                                    "Set it in appsettings.json or as an environment variable.");

            _model            = configuration["AIChatbot:GeminiModel"] ?? DefaultModel;
            _endpoint         = $"{GeminiBaseUrl}{_model}:generateContent";
            _httpClientFactory = httpClientFactory;
            _logger           = logger;

            _logger.LogInformation("GeminiLLMService configured with model '{Model}'. API key loaded.", _model);
        }

        public async Task<string> GenerateResponseAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default)
        {
            // Combine system + user prompt into a single "user" turn.
            // Gemini Flash/Pro support a top-level "system_instruction" field.
            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[]
                {
                    new
                    {
                        role  = "user",
                        parts = new[] { new { text = userPrompt } }
                    }
                },
                generationConfig = new
                {
                    temperature     = 0.3,   // lower = more factual/deterministic
                    maxOutputTokens = 512
                }
            };

            var json    = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient("GeminiClient");

            var url     = $"{_endpoint}?key={_apiKey}";
            _logger.LogInformation("Sending Gemini request to {Endpoint} with model {Model}.", _endpoint, _model);
            var sw      = System.Diagnostics.Stopwatch.StartNew();

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(url, content, cancellationToken);
                sw.Stop();
                _logger.LogInformation("Gemini API responded in {ElapsedMs}ms — status {Status}",
                    sw.ElapsedMilliseconds, (int)response.StatusCode);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Gemini API request timed out.");
                throw new TimeoutException("The AI service did not respond in time. Please try again.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Gemini API HTTP request failed.");
                throw new InvalidOperationException("Unable to reach the AI service. Please try again.", ex);
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Gemini raw response body: {Body}", responseJson);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error {Status}: {Body}", (int)response.StatusCode, responseJson);
                throw new InvalidOperationException($"AI service returned an error ({(int)response.StatusCode}). Please try again.");
            }

            // Parse: .candidates[0].content.parts[*].text
            try
            {
                using var doc  = JsonDocument.Parse(responseJson);
                var candidate = doc.RootElement
                    .GetProperty("candidates")[0];

                var parts = candidate
                    .GetProperty("content")
                    .GetProperty("parts")
                    .EnumerateArray();

                var textBuilder = new StringBuilder();
                foreach (var part in parts)
                {
                    if (part.TryGetProperty("text", out var partText)
                        && partText.ValueKind == JsonValueKind.String)
                    {
                        textBuilder.Append(partText.GetString());
                    }
                }

                var text = textBuilder.ToString().Trim();
                return !string.IsNullOrEmpty(text)
                    ? text
                    : "I could not generate a response. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini response: {Body}", responseJson);
                throw new InvalidOperationException("The AI service returned an unexpected response format.");
            }
        }
    }
}
