using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeniView.Cloud.Models
{
    public class SessionHelper 
    {
        private static void SetSession<T>(string sessionId, T value)
        {
            HttpContext.Current.Session[sessionId] = value;
        }

        private static T GetSession<T>(string sessionId, T value)
        {
            if (HttpContext.Current.Session[sessionId] != null)
            {
                return (T)HttpContext.Current.Session[sessionId];
            }
            else
            {
                return value;
            }
        }

        public static long? CommunityID
        {
            get
            {
                return GetSession<long?>("communityID",null);
            }
            set
            {
                SetSession<long?>("communityID", value);
            }
        }
        public static long? GroupID
        {
            get
            {
                return GetSession<long?>("groupID", null);
            }
            set
            {
                SetSession<long?>("groupID", value);
            }
        }
        public static bool IncludeAllSubGroups
        {
            get
            {
                return GetSession<bool>("includeAllSubGroups", true);
            }
            set
            {
                SetSession<bool>("includeAllSubGroups", value);
            }
        }
    }
}