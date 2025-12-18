// Import Namespace
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Web;

using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;

namespace GeniView.Cloud.Common
{

    /// <summary>
    ///  Hang Fire Authorization Filter Class
    /// </summary>
    /// <seealso cref="Hangfire.Dashboard.IDashboardAuthorizationFilter" />
    [ExcludeFromCodeCoverage]
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
    #region Class Method
        /// <summary>
        /// Authorizes the specified context.
        /// </summary>
        /// <param name="Context">The context.</param>
        /// <returns></returns>
        public bool Authorize(DashboardContext Context)
        {
            bool access = false;

            // In case you need an OWIN context, use the next line, `OwinContext` class is the part of the `Microsoft.Owin` package.
            var owinContext = new OwinContext(Context.GetOwinEnvironment());



            //if (owinContext.Authentication.User.Identity.Name.ToLower() == "developer"
            //    && owinContext.Authentication.User.Identity.IsAuthenticated == true)
            //{
            //    access = true;
            //}
            access = true;

            //Debug.WriteLine($"Hangfire Authorize Name:{owinContext.Authentication.User.Identity.Name } "
            //+ $"Type:{owinContext.Authentication.User.Identity.AuthenticationType} "
            //+ $"IsAuth:{owinContext.Authentication.User.Identity.IsAuthenticated} "
            //+ $"Access:{access} "
            //);

            return access;
        }
    #endregion
    }
}