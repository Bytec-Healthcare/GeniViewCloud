using System;

namespace GeniView.Cloud.Models
{
    public sealed class CentralSyncQueueItem
    {
        public long Id { get; init; }
        public Guid CommunityId { get; init; }
        public string Topic { get; init; }
        public string Payload { get; init; }
    }
}
