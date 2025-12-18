using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GeniView.Cloud.Areas.Admin.Models;
using GeniView.Cloud.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using NLog;

namespace GeniView.Cloud.Controllers
{
    [Authorize(Roles = "Community Admin")]
    public class CommunityInfoController : Controller
    {
        #region constructor
        public CommunitiesDataRepository communityRepo = new CommunitiesDataRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private UserActivityHistory userAHM = new UserActivityHistory();
        #endregion

        public ActionResult Index()
        {
            Community model = new Community();
            try
            {
                using (var identityRepo = new IdentityDataRepository())
                {
                    var currentUser = identityRepo.GetCurrentUser();
                    model = communityRepo.FindByID(currentUser.CommunityID.Value);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                ViewBag.CommunityName = "No Name";
                return View();
            }
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Community model = new Community();
            try
            {
                using (var identityRepo = new IdentityDataRepository())
                {
                    var currentUser = identityRepo.GetCurrentUser();
                    model = communityRepo.FindByID(currentUser.CommunityID.Value);
                }

                if (model == null)
                    return HttpNotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Community model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var identityRepo = new IdentityDataRepository())
                    {
                        var origModel = communityRepo.FindByID(model.ID);
                        origModel.Description = model.Description;
                        origModel.Address = model.Address;
                        communityRepo.Update(origModel);
                        userAHM.AddActivity("Edit community", ActivityObjectType.Community, origModel.Name);
                    }
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }
    }
}