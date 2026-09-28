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
            string hospitalCode,
            string topic,
            string payload,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                INSERT INTO ""CentralSyncQueue""
                (
                    ""HospitalCode"",
                    ""Topic"",
                    ""Payload""
                )
                VALUES
                (
                    @hospitalCode,
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
                "hospitalCode", hospitalCode);

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