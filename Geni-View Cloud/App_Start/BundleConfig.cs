using System.Web;
using System.Web.Optimization;

namespace GeniView.Cloud
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/dropdownfilter").Include(
                        "~/Scripts/Custom Scripts/dropdown-filters.js",
                        "~/Scripts/Custom Scripts/create-user-optional-filters.js"
                        ));

            // Battery Charts
            bundles.Add(new ScriptBundle("~/bundles/batterycharts")
                .IncludeDirectory("~/Scripts/ChartScripts/Battery", "*.js", true));

            // Device Charts
            bundles.Add(new ScriptBundle("~/bundles/devicecharts")
                .IncludeDirectory("~/Scripts/ChartScripts/Device", "*.js", true));

            // DateTime Scripts
            bundles.Add(new ScriptBundle("~/bundles/datetimescripts")
                .IncludeDirectory("~/Scripts/ChartScripts/DateTimeUIScripts", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/bootstrap-theme.min.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Admin/css").Include(
                      "~/Content/admin.min.css"));
        }
    }
}
