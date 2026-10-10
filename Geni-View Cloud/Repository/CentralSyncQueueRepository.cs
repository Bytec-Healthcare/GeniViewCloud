using GeniView.Cloud.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeniView.Cloud.Repository
{
    public class CentralSyncQueueRepository
    {
        private readonly string _connectionString;

        public CentralSyncQueueRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(
                "GeniViewCloudDataRepository");
        }

        public async Task<long> EnqueueAsync(
            Guid communityId,
            string topic,
            string payload,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO public.""CentralSyncQueue""
                (
                    ""CommunityID"",
                    ""Topic"",
                    ""Payload""
                )
                VALUES
                (
                    @communityId,
                    @topic,
                    @payload
                )
                RETURNING ""Id"";";

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("communityId", communityId);
            command.Parameters.AddWithValue("topic", topic);
            command.Parameters.AddWithValue("payload", payload);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(result);
        }

        public async Task<List<CentralSyncQueueItem>> ClaimPendingAsync(
            int batchSize,
            TimeSpan processingTimeout,
            CancellationToken cancellationToken)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            // Recover rows if the IIS process was restarted while they were being sent.
            const string recoverSql = @"
                UPDATE public.""CentralSyncQueue""
                SET ""Status"" = 'PENDING',
                    ""LastError"" = COALESCE(""LastError"", 'Recovered after processing timeout')
                WHERE ""Status"" = 'PROCESSING'
                  AND ""LastAttemptAt"" < @staleBefore;";

            await using (var recover = new NpgsqlCommand(recoverSql, connection, transaction))
            {
                recover.Parameters.AddWithValue(
                    "staleBefore",
                    DateTimeOffset.UtcNow.Subtract(processingTimeout));
                await recover.ExecuteNonQueryAsync(cancellationToken);
            }

            const string selectSql = @"
                SELECT ""Id"", ""CommunityID"", ""Topic"", ""Payload""
                FROM public.""CentralSyncQueue""
                WHERE ""Status"" = 'PENDING'
                ORDER BY ""Id"
                LIMIT @batchSize
                FOR UPDATE SKIP LOCKED;";

            var items = new List<CentralSyncQueueItem>();

            await using (var select = new NpgsqlCommand(selectSql, connection, transaction))
            {
                select.Parameters.AddWithValue("batchSize", batchSize);
                await using var reader = await select.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    items.Add(new CentralSyncQueueItem
                    {
                        Id = reader.GetInt64(0),
                        CommunityId = reader.GetGuid(1),
                        Topic = reader.GetString(2),
                        Payload = reader.GetString(3)
                    });
                }
            }

            if (items.Count > 0)
            {
                const string markProcessingSql = @"
                    UPDATE public.""CentralSyncQueue""
                    SET ""Status"" = 'PROCESSING',
                        ""AttemptCount"" = ""AttemptCount"" + 1,
                        ""LastAttemptAt"" = now()
                    WHERE ""Id"" = ANY(@ids);";

                await using var mark = new NpgsqlCommand(
                    markProcessingSql,
                    connection,
                    transaction);

                mark.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
                    .Value = items.Select(x => x.Id).ToArray();

                await mark.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return items;
        }

        public async Task MarkProcessedAsync(
            IEnumerable<long> ids,
            CancellationToken cancellationToken)
        {
            var idArray = ids.Distinct().ToArray();
            if (idArray.Length == 0) return;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                UPDATE public.""CentralSyncQueue""
                SET ""Status"" = 'PROCESSED',
                    ""ProcessedAt"" = now(),
                    ""LastError"" = NULL
                WHERE ""Id"" = ANY(@ids);";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
                .Value = idArray;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task MarkFailedAsync(
            IEnumerable<long> ids,
            string error,
            CancellationToken cancellationToken)
        {
            var idArray = ids.Distinct().ToArray();
            if (idArray.Length == 0) return;

            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                UPDATE public.""CentralSyncQueue""
                SET ""Status"" = 'PENDING',
                    ""LastError"" = @error
                WHERE ""Id"" = ANY(@ids);";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Add("ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint)
                .Value = idArray;
            command.Parameters.AddWithValue(
                "error",
                string.IsNullOrEmpty(error)
                    ? "Unknown central sync error"
                    : error.Length > 4000 ? error[..4000] : error);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
