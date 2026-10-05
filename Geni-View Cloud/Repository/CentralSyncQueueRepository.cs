using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GeniView.Cloud.Repository
{
    public class CentralSyncQueueRepository
    {
        private readonly string _connectionString;

        public CentralSyncQueueRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "GeniViewCloudDataRepository");
        }

        public async Task<long> EnqueueAsync(
            Guid communityId,
            string topic,
            string payload,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO ""CentralSyncQueue""
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

            await using var connection =
                new NpgsqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command =
                new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "communityId",communityId);

            command.Parameters.AddWithValue(
                "topic", topic);

            command.Parameters.AddWithValue(
                "payload", payload);

            var result =
                await command.ExecuteScalarAsync(cancellationToken);

            return Convert.ToInt64(result);
        }
    }
}