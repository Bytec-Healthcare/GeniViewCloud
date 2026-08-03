using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public class AIChatController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "AI Battery Assistant";
            return View();
        }
    }
}
