using GeniView.Cloud.Common;
using GeniView.Cloud.Repository;
using Hangfire;
using Microsoft.Owin;
using Owin;


[assembly: OwinStartupAttribute(typeof(GeniView.Cloud.Startup))]
namespace GeniView.Cloud
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            HangfireRepository hangfire = new HangfireRepository();

            if (Global._enableHangFire)
            {
                app.UseHangfireDashboard
                (
                    "/hangfire",
                    new DashboardOptions
                    {
                        Authorization = new[] { new HangFireAuthorizationFilter() }
                    }
                );
            }
        }
    }
}
