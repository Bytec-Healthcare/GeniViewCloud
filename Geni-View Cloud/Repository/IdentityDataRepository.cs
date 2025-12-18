using GeniView.Cloud.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using GeniView.Data.Web;

namespace GeniView.Cloud.Repository
{
    public class IdentityDataRepository : IDisposable
    {
        #region Users
        public List<UserViewModel> GetUsers(long? communityID = null, long? groupID = null)
        {
            using (var db = new ApplicationDbContext())
            {
                List<UserViewModel> model = new List<UserViewModel>();
                var query = db.Users.Include(x => x.Roles)
                                    .Where(r => (communityID == null ? true : r.CommunityID == communityID))
                                    .ToList();

                var groups = new List<Group>();
                var communities = new List<Community>();

                using (var gRepo = new GroupsDataRepository())
                {
                    groups = gRepo.GetGroups(communityID, groupID);
                }

                using (var cRepo = new CommunitiesDataRepository())
                {
                    communities = cRepo.GetCommunities();
                }

                if (groupID != null)
                {
                    model = (from u in query
                             join g in groups on u.GroupID equals g.ID into tmp_g
                             from grp in tmp_g
                             join c in communities on u.CommunityID equals c.ID into tmp_c
                             from community in tmp_c.DefaultIfEmpty(new Community { Name = "" })
                             join role in db.Roles on u.Roles.Select(rr => rr.RoleId).First() equals role.Id
                             select new UserViewModel
                             {
                                 GroupName = grp.Name,
                                 CommunityName = community.Name,
                                 RoleName = role.Name,
                                 isUserLocked = u.LockoutEndDateUtc == null ? false : u.LockoutEndDateUtc.Value > DateTime.UtcNow ? true : false,
                                 User = u
                             }).ToList();

                }
                else
                {
                    model = (from u in query
                             join g in groups on u.GroupID equals g.ID into tmp_g
                             from grp in tmp_g.DefaultIfEmpty(new Group { Name = "" })
                             join c in communities on u.CommunityID equals c.ID into tmp_c
                             from community in tmp_c.DefaultIfEmpty(new Community { Name = "" })
                             join role in db.Roles on u.Roles.Select(rr => rr.RoleId).First() equals role.Id
                             select new UserViewModel
                             {
                                 GroupName = grp.Name,
                                 CommunityName = community.Name,
                                 RoleName = role.Name,
                                 isUserLocked = u.LockoutEndDateUtc == null ? false : u.LockoutEndDateUtc.Value > DateTime.UtcNow ? true : false,
                                 User = u
                             }).ToList();
                }

                return model;
            }

        }

        public List<UserViewModel> GetUsersWhoHasAccess(long? communityID = null, long? groupID = null)
        {
            using (var db = new ApplicationDbContext())
            {
                List<UserViewModel> model = new List<UserViewModel>();
                List<Group> groups = new List<Group>();
                List<Community> communities = new List<Community>();


                var query = (from u in db.Users.Include(x => x.Roles)
                             join r in db.Roles on u.Roles.FirstOrDefault().RoleId equals r.Id
                             where u.CommunityID == communityID ||
                                   r.Name.Contains("Application")
                             select u).ToList();


                using (var gRepo = new GroupsDataRepository())
                {
                    groups = gRepo.GetGroups(communityID, groupID);
                }

                using (var cRepo = new CommunitiesDataRepository())
                {
                    communities = cRepo.GetCommunities();
                }

                // Optional Filter
                if (groupID != null)
                {
                    model = (from u in query
                             join g in groups on u.GroupID equals g.ID into tmp_g
                             from grp in tmp_g
                             join c in communities on u.CommunityID equals c.ID into tmp_c
                             from community in tmp_c.DefaultIfEmpty(new Community { Name = "" })
                             join role in db.Roles on u.Roles.Select(rr => rr.RoleId).First() equals role.Id
                             select new UserViewModel
                             {
                                 GroupName = grp.Name,
                                 CommunityName = community.Name,
                                 RoleName = role.Name,
                                 isUserLocked = u.LockoutEndDateUtc == null ? false : u.LockoutEndDateUtc.Value > DateTime.UtcNow ? true : false,
                                 User = u
                             }).ToList();

                }
                else
                {
                    model = (from u in query
                             join g in groups on u.GroupID equals g.ID into tmp_g
                             from grp in tmp_g.DefaultIfEmpty(new Group { Name = "" })
                             join c in communities on u.CommunityID equals c.ID into tmp_c
                             from community in tmp_c.DefaultIfEmpty(new Community { Name = "" })
                             join role in db.Roles on u.Roles.Select(rr => rr.RoleId).First() equals role.Id
                             select new UserViewModel
                             {
                                 GroupName = grp.Name,
                                 CommunityName = community.Name,
                                 RoleName = role.Name,
                                 isUserLocked = u.LockoutEndDateUtc == null ? false : u.LockoutEndDateUtc.Value > DateTime.UtcNow ? true : false,
                                 User = u
                             }).ToList();
                }

                return model;
            }

        }

        public ApplicationUser GetCurrentUser()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            }
        }

        public List<ApplicationUser> GetUsersByGroupID(long groupID)
        {
            using (var userdb = new ApplicationDbContext())
            {
                userdb.Configuration.LazyLoadingEnabled = false;
                userdb.Configuration.AutoDetectChangesEnabled = false;
                userdb.Configuration.ProxyCreationEnabled = false;

                return userdb.Users.Where(x => x.GroupID == groupID).ToList();
            }
        }

        public ApplicationUser FindUserByID(string id, long? communityID = null, long? groupID = null)
        {
            using (var userdb = new ApplicationDbContext())
            {
                userdb.Configuration.LazyLoadingEnabled = false;
                return userdb.Users
                             .Where(x => (communityID == null ? true : x.CommunityID == communityID) &&
                                         (groupID == null ? true : x.GroupID == groupID) && x.Id == id)
                             .FirstOrDefault();
            }
        }

        public List<IdentityRole> GetRoles()
        {
            using (var userdb = new ApplicationDbContext())
            {
                userdb.Configuration.LazyLoadingEnabled = false;
                userdb.Configuration.AutoDetectChangesEnabled = false;
                userdb.Configuration.ProxyCreationEnabled = false;

                return userdb.Roles.ToList();
            }
        }

        public void AddActivity(UserActivityHistory model)
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                db.UserActivityHistory.Add(model);
                db.SaveChanges();
            }
        }

        public List<UserActivityHistory> GetActivities(UserActivityHistoryFilter filter, ApplicationUser currentUser)
        {
            List<UserActivityHistory> model = new List<UserActivityHistory>();

            using (var db = new GeniViewCloudDataRepository())
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;
                var convertedBeginDate = TimeZoneHelper.ConvertToUTC(filter.BeginDate, currentUser);
                var convertedEndDate = TimeZoneHelper.ConvertToUTC(filter.EndDate, currentUser);

                model = db.UserActivityHistory.Where(x => x.Timestamp >= convertedBeginDate &&
                                                          x.Timestamp <= convertedEndDate)
                                              .OrderByDescending(x => x.Timestamp)
                                              .Take(filter.Count)
                                              .ToList();
            }

            return model;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}