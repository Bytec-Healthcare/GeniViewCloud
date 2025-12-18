using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Common
{
    [ExcludeFromCodeCoverage]
    public class ApplicationPreload : System.Web.Hosting.IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
            Debug.WriteLine("ApplicationPreload Preload");
            HangfireBootstrapper.Instance.Start();
        }
    }
}