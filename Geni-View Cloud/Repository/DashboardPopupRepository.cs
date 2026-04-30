using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public sealed class DashboardPopupRepository : IDisposable
    {
        private readonly GeniViewCloudDataRepository _db;

        public DashboardPopupRepository(GeniViewCloudDataRepository db)
        {
            _db = db;
        }

        public (List<DashboardPopupRowModel> Items, int Total) GetPopupDashboardRows(
            HashSet<long> batteryIds,
            string search,
            int pageNumber,
            int pageSize)
        {
            if (batteryIds == null || batteryIds.Count == 0)
                return (new List<DashboardPopupRowModel>(), 0);

            var db = _db;
            db.Database.SetCommandTimeout(500);

            var allRows = db.Database
                .SqlQueryRaw<DashboardPopupRowModel>(
                    """SELECT * FROM sp_popupDashboard(@ids)""",
                    new NpgsqlParameter("ids", batteryIds.ToArray())
                    {
                        NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Bigint
                    })
                .ToList();

            IEnumerable<DashboardPopupRowModel> filtered = allRows;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                filtered = filtered.Where(r =>
                    (!string.IsNullOrEmpty(r.PowerModules) && r.PowerModules.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(r.AttachedTo)   && r.AttachedTo.IndexOf(s,   StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(r.DeviceType)   && r.DeviceType.IndexOf(s,   StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrEmpty(r.Status)       && r.Status.IndexOf(s,       StringComparison.OrdinalIgnoreCase) >= 0));
            }

            var total = filtered.Count();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1)   pageSize   = 10;

            var items = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, total);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
