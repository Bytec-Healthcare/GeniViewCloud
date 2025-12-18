using GeniView.Cloud.Areas.Admin.Models;
using GeniView.Cloud.Common;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GeniView.Cloud.Models
{
    public class TimeZoneHelper
    {
        public static string GetLocalDateTime(DateTime dateTime)
        {
            using (var userDb = new ApplicationDbContext())
            {
                var currentUser = userDb.Users.Find(HttpContext.Current.User.Identity.GetUserId());
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneId)).ToString(Global.dateTimeFormat);
            }
        }
        // Get Local Date
        public static string GetLocalDate(DateTime dateTime)
        {
            using (var userDb = new ApplicationDbContext())
            {
                var currentUser = userDb.Users.Find(HttpContext.Current.User.Identity.GetUserId());
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneId)).ToShortDateString();
            }
        }
        public static string GetLocalDateTime(DateTime dateTime, ApplicationUser currentUser)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneId)).ToString(Global.dateTimeFormat);
        }

        public static string GetLocalDate(DateTime dateTime, ApplicationUser currentUser)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneId)).ToShortDateString();
        }

        public static DateTime ConvertToUTC(DateTime dateTime, ApplicationUser currentUser)
        {
           return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneId));
        }

        // Get Time Zones
        public static List<TimeZoneInfoHelper> GetTimeZoneList()
        {
            ReadOnlyCollection<TimeZoneInfo> tz = TimeZoneInfo.GetSystemTimeZones();
            var timeZoneList = from b in tz
                        select new TimeZoneInfoHelper
                        {
                            ID = b.Id,
                            DisplayText = b.DisplayName
                        };
            return timeZoneList.ToList();
        }

        public class TimeZoneInfoHelper
        {
            public string ID { get; set; }
            public string DisplayText { get; set; }
        }
    }

}