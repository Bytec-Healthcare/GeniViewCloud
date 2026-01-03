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

        // Add in DashboardController class (anywhere near other JsonResult endpoints)
        public JsonResult GetCycleStatus()
        {
            var model = new CycleStatusModel();
            var currentUser = UserManager.FindById(User.Identity.GetUserId());

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetCycleStatus(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetCycleStatus(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetCycleStatus(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetCycleStatus(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                    }
                    else
                    {
                        model = db.GetCycleStatus(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                    }
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error("GetCycleStatus failed.", ex);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetStateOfCharge()
        {
            var model = new StateOfChargeModel();
            var currentUser = UserManager.FindById(User.Identity.GetUserId());

            try
            {
                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    model = db.GetStateOfCharge(SessionHelper.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Admin"))
                {
                    model = db.GetStateOfCharge(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    model = db.GetStateOfCharge(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                }
                else if (User.IsInRole("Community User"))
                {
                    if (currentUser.GroupID != null)
                    {
                        model = db.GetStateOfCharge(currentUser.CommunityID, currentUser.GroupID, SessionHelper.IncludeAllSubGroups);
                    }
                    else
                    {
                        model = db.GetStateOfCharge(currentUser.CommunityID, SessionHelper.GroupID, SessionHelper.IncludeAllSubGroups);
                    }
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error("GetStateOfCharge failed.", ex);
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