using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Application Admin")]
    public class ApplicationLogsController : Controller
    {
        private readonly IdentityDataRepository _identityRepo;
        private readonly ApplicationLogsDataRepository _logsRepo;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public ApplicationLogsController(IdentityDataRepository identityRepo, ApplicationLogsDataRepository logsRepo)
        {
            _identityRepo = identityRepo;
            _logsRepo = logsRepo;
        }
        public ActionResult Index()
        {
            // Default to last 2 hours (UX match with other history/graph screens).
            var now = DateTime.Now;
            var query = new ApplicationLogsFilter()
            {
                BeginDate = now.AddHours(-2),
                EndDate = now,
                Count = 50,
                LogLevel = ApplicationLogLevel.ALL,
                ApplicationLogList = null
            };
            return View(query);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ApplicationLogsFilter model)
        {
            var now = DateTime.Now;
            ApplicationLogsFilter query = new ApplicationLogsFilter()
            {
                BeginDate = model.BeginDate == DateTime.MinValue ? now.AddHours(-2) : model.BeginDate,
                EndDate = model.EndDate == DateTime.MinValue ? now : model.EndDate,
                Count = model.Count < 0 ? 100 : model.Count,
                LogLevel = model.LogLevel
            };

            try
            {
                ApplicationUser currentUser = new ApplicationUser();
                var identityRepo = _identityRepo;
                {
                    currentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                    ViewBag.CurrentUser = currentUser;
                }
                var db = _logsRepo;
                {
                    query.ApplicationLogList = db.GetApplicationLogs(query, currentUser);
                }

                return View(query);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(query);
            }
        }

    }
}
