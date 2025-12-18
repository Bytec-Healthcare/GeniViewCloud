using System.Linq;
using System.Web.Mvc;

namespace GeniView.Cloud.Areas.Admin.Models
{
    public class NewPartialViewEngine : RazorViewEngine
    {
        private static readonly string[] NEW_PARTIAL_FOLDER = new[]
        {
            "~/Views/PartialViews/{0}.cshtml"
        };

        public NewPartialViewEngine()
        {
            base.PartialViewLocationFormats = base.PartialViewLocationFormats.Union(NEW_PARTIAL_FOLDER).ToArray();
        }
    }
}