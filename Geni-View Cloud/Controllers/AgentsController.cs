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
using GeniView.Data.Agent;
using NLog;

namespace GeniView.Cloud.Controllers
{
    [Authorize(Roles = "Application Admin, Application User")]
    public class AgentsController : Controller
    {
        #region constructor
        public AgentsDataRepository agentsRepo = new AgentsDataRepository();
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        #endregion

        public ActionResult Index()
        {
            try
            {
                return View(agentsRepo.GetAgents());
            }
            catch (Exception ex)
            {
                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }
        protected override void Dispose(bool disposing)
        {
            agentsRepo.Dispose();
            base.Dispose(disposing);
        }
    }
}
