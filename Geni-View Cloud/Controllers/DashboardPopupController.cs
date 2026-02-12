using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
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
    [Authorize]
    public sealed class DashboardPopupController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        [HttpGet]
        public JsonResult GetSocPopupData(string cardKey, string search, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var currentUser = UserManager.FindById(User.Identity.GetUserId());

                long? communityId = null;
                long? groupId = null;
                var includeAllSubGroups = SessionHelper.IncludeAllSubGroups;

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    communityId = SessionHelper.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID;
                }
                else if (User.IsInRole("Community User"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID ?? SessionHelper.GroupID;
                }

                using (var repo = new DashboardDataRepository())
                using (var popupRepo = new DashboardPopupRepository())
                {
                    var soc = repo.GetStateOfCharge(communityId, groupId, includeAllSubGroups);

                    IEnumerable<long> ids = Enumerable.Empty<long>();
                    var key = (cardKey ?? string.Empty).Trim().ToLowerInvariant();

                    if (key == "high") ids = soc.HighSoCBatteryIds;
                    else if (key == "low") ids = soc.LowSoCBatteryIds;
                    else if (key == "chargenow") ids = soc.ChargeNowBatteryIds;

                    var idSet = new HashSet<long>(ids ?? new List<long>());
                    var (items, total) = popupRepo.GetPopupDashboardRows(idSet, search, pageNumber, pageSize);

                    return Json(new
                    {
                        Success = true,
                        Items = items,
                        Total = total,
                        PageNumber = pageNumber,
                        PageSize = pageSize,

                        // Must be the number of Battery_IDs for the clicked card (not page size)
                        PowerModulesCount = idSet.Count
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetSocPopupData failed.");

                return Json(new
                {
                    Success = false,
                    ErrorMessage = ex.GetBaseException().Message,
                    Items = new List<DashboardPopupRowModel>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    PowerModulesCount = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}