using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public class DeviceEventsController : Controller
    {
        // GET: DeviceEvents
        public ActionResult Index()
        {
            return View();
        }
    }
}