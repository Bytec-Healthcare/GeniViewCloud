using GeniView.Cloud.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class ApplicationUpdatesDataRepository : IDisposable
    {
        private readonly GeniViewCloudDataRepository _db;

        public ApplicationUpdatesDataRepository(GeniViewCloudDataRepository db)
        {
            _db = db;
        }

        public List<ApplicationUpdate> GetUpdates()
        {
            var db = _db;
            {

                return db.ApplicationUpdates.ToList();
            }
        }

        public void Update(ApplicationUpdate model)
        {
            var db = _db;
            {

                ApplicationUpdate originalAppUpdate = db.ApplicationUpdates.Find(model.ID);

                originalAppUpdate.HasUpdate = model.HasUpdate;
                originalAppUpdate.LatestVersion = model.LatestVersion;
                originalAppUpdate.ReleaseDate = model.ReleaseDate;
                originalAppUpdate.Description = model.Description;
                originalAppUpdate.ReleaseNotes = model.ReleaseNotes;
                originalAppUpdate.DownloadAddress = model.DownloadAddress;

                db.Entry(originalAppUpdate).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public ApplicationUpdate FindById(long appId)
        {
            var db = _db;
            {
                return db.ApplicationUpdates.Find(appId);
            }
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}