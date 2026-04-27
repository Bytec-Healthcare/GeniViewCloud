using GeniView.Data.Web;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class CommunitiesDataRepository : IDisposable
    {
        private readonly GeniViewCloudDataRepository _db;

        public CommunitiesDataRepository(GeniViewCloudDataRepository db)
        {
            _db = db;
        }

        #region Communities
        public List<Community> GetCommunities()
        {
            var db = _db;
            {

                return db.Communities.AsNoTracking().ToList();
            }
        }

        public Community FindByID(long id)
        {
            var db = _db;
            {

                return db.Communities.Find(id);
            }
        }

        public void Insert(Community community)
        {
            var db = _db;
            {
                // Setting Creation date and new GUID
                community.CreateDate = DateTime.UtcNow;
                community.CommunityID = Guid.NewGuid();

                db.Communities.Add(community);
                db.SaveChanges();
            }
        }

        public void Update(Community community)
        {
            var db = _db;
            {
                var originalCommunity = db.Communities.Find(community.ID);
                
                originalCommunity.Name = community.Name;
                originalCommunity.Description = community.Description;
                originalCommunity.IsActive = community.IsActive;
                originalCommunity.Address = community.Address != null ? community.Address : originalCommunity.Address;

                db.Entry(originalCommunity).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}