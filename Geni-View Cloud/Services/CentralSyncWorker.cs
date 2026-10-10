using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GeniView.Cloud.Services
{
    /// <summary>
    /// Runs inside the existing IIS-hosted GeniView Cloud application.
    /// It reads the local CentralSyncQueue and sends batches to the central sync API.
    /// Disabled until the central API endpoint is available.
    /// </summary>
    public sealed class CentralSyncWorker : BackgroundService
    {
        private readonly CentralSyncQueueRepository _queue;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CentralSyncWorker> _logger;

        public CentralSyncWorker(
            CentralSyncQueueRepository queue,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<CentralSyncWorker> logger)
        {
            _queue = queue;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = _configuration.GetValue("CentralSync:Enabled", false);
            if (!enabled)
            {
                _logger.LogInformation("Central Sync Worker is disabled. Queue collection remains active.");
                return;
            }

            var apiUrl = _configuration["CentralSync:CentralApiUrl"];
            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                _logger.LogError("Central Sync Worker is enabled but CentralSync:CentralApiUrl is missing.");
                return;
            }

            var batchSize = Math.Clamp(_configuration.GetValue("CentralSync:BatchSize", 50), 1, 500);
            var pollSeconds = Math.Clamp(_configuration.GetValue("CentralSync:PollIntervalSeconds", 5), 1, 300);
            var timeoutSeconds = Math.Clamp(_configuration.GetValue("CentralSync:HttpTimeoutSeconds", 30), 5, 300);
            var processingTimeoutMinutes = Math.Clamp(
                _configuration.GetValue("CentralSync:ProcessingTimeoutMinutes", 10), 1, 1440);

            var client = _httpClientFactory.CreateClient("CentralSync");
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

            _logger.LogInformation(
                "Central Sync Worker started. API={ApiUrl}, BatchSize={BatchSize}, PollInterval={PollSeconds}s",
                apiUrl, batchSize, pollSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var items = await _queue.ClaimPendingAsync(
                        batchSize,
                        TimeSpan.FromMinutes(processingTimeoutMinutes),
                        stoppingToken);

                    if (items.Count > 0)
                    {
                        await SendBatchesAsync(items, apiUrl, client, stoppingToken);
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(pollSeconds), stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Central Sync Worker cycle failed.");
                    await Task.Delay(TimeSpan.FromSeconds(pollSeconds), stoppingToken);
                }
            }

            _logger.LogInformation("Central Sync Worker stopped.");
        }

        private async Task SendBatchesAsync(
            System.Collections.Generic.List<CentralSyncQueueItem> items,
            string apiUrl,
            HttpClient client,
            CancellationToken cancellationToken)
        {
            // Keep each CommunityID in its own request so central tenant routing is explicit.
            foreach (var communityGroup in items.GroupBy(x => x.CommunityId))
            {
                var groupItems = communityGroup.ToList();
                var batch = new CentralSyncBatch
                {
                    CommunityId = communityGroup.Key,
                    Messages = groupItems.Select(x => new CentralSyncBatchMessage
                    {
                        QueueId = x.Id,
                        Topic = x.Topic,
                        Payload = x.Payload
                    }).ToList()
                };

                try
                {
                    using var response = await client.PostAsJsonAsync(
                        apiUrl,
                        batch,
                        cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        await _queue.MarkProcessedAsync(
                            groupItems.Select(x => x.Id),
                            cancellationToken);

                        _logger.LogInformation(
                            "Central Sync: sent {Count} messages for CommunityID={CommunityId}.",
                            groupItems.Count,
                            communityGroup.Key);
                    }
                    else
                    {
                        var body = await response.Content.ReadAsStringAsync(cancellationToken);
                        var error = $"Central API returned {(int)response.StatusCode} {response.ReasonPhrase}. {body}";

                        await _queue.MarkFailedAsync(
                            groupItems.Select(x => x.Id),
                            error,
                            cancellationToken);

                        _logger.LogWarning(
                            "Central Sync failed for CommunityID={CommunityId}: {Error}",
                            communityGroup.Key,
                            error);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    await _queue.MarkFailedAsync(
                        groupItems.Select(x => x.Id),
                        ex.Message,
                        cancellationToken);

                    _logger.LogWarning(
                        ex,
                        "Central Sync connection failed for CommunityID={CommunityId}. Messages returned to PENDING.",
                        communityGroup.Key);
                }
            }
        }
    }
}
