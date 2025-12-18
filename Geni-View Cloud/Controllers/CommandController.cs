using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GeniView.Cloud.Controllers
{
    public class CommandController : Controller
    {
        // GET: Command
        public ActionResult Index(string id)
        {
            return View();
        }
    }
}