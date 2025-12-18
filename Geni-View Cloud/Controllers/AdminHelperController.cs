using GeniView.Cloud.Areas.Admin.Models;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GeniView.Cloud.Controllers
{
    [Authorize(Roles = "Application Admin,Community Admin,Community Group Admin")]
    public class AdminHelperController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        #region DropDown Logic
        public JsonResult LoadCommunitiesList()
        {
            List<Community> model = new List<Community>();
            var emptyList = Enumerable.Empty<SelectList>();
            try
            {
                var currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.GetCurrentUser();
                }

                if (User.Identity.IsAuthenticated)
                {
                    using (var db = new CommunitiesDataRepository())
                    {
                        model = db.GetCommunities();
                    }

                    if (User.IsInRole("Application Admin"))
                    {
                        return Json(new SelectList(model, "ID", "Name"), JsonRequestBehavior.AllowGet);
                    }
                    else if (User.IsInRole("Community Admin") || User.IsInRole("Community Group Admin"))
                    {
                        model = model.Where(x => x.ID == currentUser.CommunityID).ToList();
                        return Json(new SelectList(model, "ID", "Name"), JsonRequestBehavior.AllowGet);
                    }
                    return Json(emptyList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(emptyList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return Json(emptyList, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LoadGroupsList(long communityID)
        {
            List<Group> model = new List<Group>();
            var emptyList = Enumerable.Empty<SelectList>();

            try
            {
                var currentUser = new ApplicationUser();
                using (var identityRepo = new IdentityDataRepository())
                {
                    currentUser = identityRepo.GetCurrentUser();
                }

                if (User.Identity.IsAuthenticated)
                {

                    using (var db = new GroupsDataRepository())
                    {
                        if (User.IsInRole("Application Admin") || User.IsInRole("Community Admin"))
                        {
                            model = db.GetGroups(communityID);
                            return Json(new SelectList(model, "ID", "Name"), JsonRequestBehavior.AllowGet);
                        }
                        if (User.IsInRole("Community Group Admin"))
                        {
                            model = db.GetGroups(communityID, currentUser.GroupID);
                            return Json(new SelectList(model, "ID", "Name"), JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(emptyList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(emptyList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return Json(emptyList, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LoadRolesList()
        {
            var emptyList = Enumerable.Empty<SelectList>();

            try
            {
                var currentUser = new ApplicationUser();
                using (var db = new IdentityDataRepository())
                {
                    currentUser = db.GetCurrentUser();

                    if (User.Identity.IsAuthenticated)
                    {
                        if (User.IsInRole("Application Admin"))
                        {
                            return Json(new SelectList(db.GetRoles(), "Name", "Name"), JsonRequestBehavior.AllowGet);
                        }
                        else if (User.IsInRole("Community Admin"))
                        {
                            return Json(new SelectList(db.GetRoles().Where(x => x.Name.Contains("Community")), "Name", "Name"), JsonRequestBehavior.AllowGet);
                        }
                        else if (User.IsInRole("Community Group Admin"))
                        {
                            return Json(new SelectList(db.GetRoles().Where(x => x.Name.Contains("Community") && !x.Name.Contains("Community Admin")), "Name", "Name"), JsonRequestBehavior.AllowGet);
                        }
                        return Json(emptyList, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(emptyList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return Json(emptyList, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Session
        public JsonResult SaveFilterState(long? communityID, long? groupID, bool includeAllSubGroups)
        {
            SessionHelper.CommunityID = communityID;
            SessionHelper.GroupID = groupID;
            SessionHelper.IncludeAllSubGroups = includeAllSubGroups;

            return Json(new { success = true });
        }

        public JsonResult GetFilterState()
        {
            return Json(
                new
                {
                    communityID = SessionHelper.CommunityID,
                    groupID = SessionHelper.GroupID,
                    includeAllSubGroups = SessionHelper.IncludeAllSubGroups != null ? SessionHelper.IncludeAllSubGroups : false
                },
                JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}