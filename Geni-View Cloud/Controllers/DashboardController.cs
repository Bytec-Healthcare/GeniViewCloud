using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeniView.Cloud.Hubs;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace GeniView.Cloud.Controllers
{

    [System.Web.Mvc.Authorize]
    public class DashboardController : Controller
    {
        #region constructor
        public DashboardDataRepository db = new DashboardDataRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }
        #endregion

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            try
            {
                var currentUser = new ApplicationUser();
                using (var userDb = new IdentityDataRepository())
                {
                    currentUser = userDb.GetCurrentUser();
                    ViewBag.CurrentUser = currentUser;
                }

                Community community = new Community();

                using (var comDb = new CommunitiesDataRepository())
                {
                    if (currentUser.CommunityID != null)
                        community = comDb.FindByID(currentUser.CommunityID.Value);
                }

                return View(community);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        public JsonResult GetDeviceDashboardChartData()
        {
            DeviceDashboardModel model = new DeviceDashboardModel();
            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetDeviceDashboardChartData(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetDeviceDashboardChartData(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetDeviceDashboardChartData(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community User"))
                {
                    // Community User can be with or without group assigned.
                    if (currentUser.GroupID != null)
                        model = db.GetDeviceDashboardChartData(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                    else
                        model = db.GetDeviceDashboardChartData(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);

                }
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetBatteryDashboardChartData()
        {
            BatteryDashboardModel model = new BatteryDashboardModel();
            var currentUser = UserManager.FindById(User.Identity.GetUserId());

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetBatteryDashboardChartData(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetBatteryDashboardChartData(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetBatteryDashboardChartData(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                        model = db.GetBatteryDashboardChartData(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                    else
                        model = db.GetBatteryDashboardChartData(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);

                }
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RedirectFilteredPage(string selectedItem, string category)
        {
            string redirectLink;

            if (category == "battery")
            {
                switch (selectedItem)
                {
                    case "Idle, Needs Charging":
                        selectedItem = "IdleNeedsCharging";
                        break;
                    case "Idle, Ready to Use":
                        selectedItem = "IdleReadytoUse";
                        break;
                }
                redirectLink = Url.Action("Index", "Batteries", new { id = selectedItem });
            }
            else
            {
                switch (selectedItem)
                {
                    case "Online, On Battery":
                        selectedItem = "OnBattery";
                        break;
                    case "Online, Plugged in":
                        selectedItem = "PluggedIn";
                        break;
                }
                redirectLink = Url.Action("Index", "Devices", new { id = selectedItem });
            }
            return Json(new { Url = redirectLink }, JsonRequestBehavior.AllowGet);
        }

        // Get Online Device and Battery Charts Data in last 14 days
        public JsonResult GetOnlineChartData()
        {
            List<IEnumerable<OnlineItemsChartModel>> model = new List<IEnumerable<OnlineItemsChartModel>>();
            JsonResult jsonResult;
            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model.Add(db.GetDeviceOnlineChartData(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups));
                    model.Add(db.GetBatteryOnlineChartData(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups));
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model.Add(db.GetDeviceOnlineChartData(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups));
                    model.Add(db.GetBatteryOnlineChartData(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups));
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model.Add(db.GetDeviceOnlineChartData(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups));
                    model.Add(db.GetBatteryOnlineChartData(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups));
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model.Add(db.GetDeviceOnlineChartData(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups));
                        model.Add(db.GetBatteryOnlineChartData(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups));
                    }
                    else
                    {
                        model.Add(db.GetDeviceOnlineChartData(currentUser.CommunityID, null, SessionHelper.IncludeAllSubGroups));
                        model.Add(db.GetBatteryOnlineChartData(currentUser.CommunityID, null, SessionHelper.IncludeAllSubGroups));
                    }
                }
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetDeviceEventHistory()
        {
            List<DeviceEvent> model = new List<DeviceEvent>();
            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            ViewBag.currentUser = currentUser;
            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetDeviceEventHistory(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetDeviceEventHistory(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetDeviceEventHistory(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                        model = db.GetDeviceEventHistory(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                    else
                        model = db.GetDeviceEventHistory(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
            }
            return PartialView("_DashboardDeviceEventHistory", model);
        }

        public JsonResult GetLocations()
        {
            List<DeviceLocationViewModel> model = new List<DeviceLocationViewModel>();
            try
            {
                var currentUser = UserManager.FindById(User.Identity.GetUserId());

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetDevicesLocation(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetDevicesLocation(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetDevicesLocation(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                        model = db.GetDevicesLocation(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                    else
                        model = db.GetDevicesLocation(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);

                }
                var jsonResult = Json(model, JsonRequestBehavior.AllowGet);
                return jsonResult;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Get Location Failed");
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBingMapKey()
        {
            string result = GlobalSettings.BingMapKey;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}