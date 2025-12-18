using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using GeniView.Cloud.Repository;
using System.Web;
using GeniView.Data.Hardware.Event;
using GeniView.Data.Hardware;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System;
using System.Data.Entity;
using NLog;
using Microsoft.AspNet.SignalR;
using GeniView.Cloud.Hubs;

namespace GeniView.Cloud.Models
{
    public class DeviceEventMailNotifier
    {
        MailHelper mailHelper;
        List<UserViewModel> users;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

        public DeviceEventMailNotifier()
        {
            this.mailHelper = new MailHelper();
        }

        public async Task SendMessageAsync(IEnumerable<DeviceEvent> deviceEvents)
        {
            foreach (var item in deviceEvents)
            {
                await SendMessageAsync(item);
            }
        }

        public async Task SendMessageAsync(DeviceEvent deviceEvent)
        {
            Device originDevice;
            bool isMessageSent = false;
            if (CheckEventRules(deviceEvent))
            {
                try
                {
                    using (DevicesDataRepository deviceDb = new DevicesDataRepository())
                    {
                        originDevice = deviceDb.FindBySN(deviceEvent.DeviceSerialNumber);
                    }

                    if (originDevice != null && originDevice.IsDeactivated == false)
                    {
                        long? nullableLong = null;
                        using (IdentityDataRepository userDb = new IdentityDataRepository())
                        {
                            this.users = userDb.GetUsersWhoHasAccess(originDevice.Community != null ? originDevice.Community.ID : nullableLong,
                                                                     originDevice.Group != null ? originDevice.Group.ID : nullableLong
                                              ).ToList();
                        }

                        // Send e-mail notification
                        foreach (var user in users)
                        {
                            if (user.User.Email == "admin@bytec.com")
                                continue;

                            // Check is Notification enabled
                            if (!user.User.IsNotificationEnable)
                                continue;
                            try
                            {
                                // SignalR push notification to spec users.
                                await context.Clients.User(user.User.Email).addNotifcation(deviceEvent.DeviceSerialNumber,
                                                                                           deviceEvent.Description,
                                                                                           TimeZoneHelper.GetLocalDateTime(deviceEvent.Timestamp, user.User));
                                await mailHelper.SendMailAsync(user.User.Email, deviceEvent);
                                isMessageSent = true;
                                await Task.Delay(GlobalSettings.NotificationDelayTimeInSeconds * 1000);

                            }
                            catch (Exception ex)
                            {
                                _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                            }
                        }

                        if (isMessageSent)
                        {
                            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
                            {
                                try
                                {
                                    DateTime oldestNotifiableEventDate = DateTime.UtcNow.AddMinutes(GlobalSettings.NotificationToleranceInMinutes * -1);
                                    DeviceEvent originEvent = db.DeviceEvents
                                                                .AsEnumerable()
                                                                .Where(d => d.Timestamp == deviceEvent.Timestamp
                                                                         && d.DeviceSerialNumber == deviceEvent.DeviceSerialNumber
                                                                         && d.Timestamp > oldestNotifiableEventDate)
                                                                .FirstOrDefault();
                                    if (originEvent != null)
                                    {
                                        originEvent.IsHandled = true;
                                        db.Entry(originEvent).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                }
            }
        }

        private bool CheckSendMessage(UserViewModel user, Device originDevice)
        {
            if (!user.User.IsNotificationEnable)
                return false;

            if ((user.RoleName == "Application User" || user.RoleName == "Application Admin"))
                return true;
            else if (user.RoleName == "Community Admin" && user.User.CommunityID == originDevice.Community.ID)
                return true;

            if (originDevice.Group != null)
            {
                if ((user.RoleName == "Community Group Admin" || user.RoleName == "Community User") &&
                     user.User.CommunityID == originDevice.Community.ID &&
                     user.User.GroupID == originDevice.Group.ID)
                    return true;
            }
            else
            {
                if (user.RoleName == "Community User" && user.User.CommunityID == originDevice.Community.ID)
                    return true;
            }
            return false;
        }

        private bool CheckEventRules(DeviceEvent deviceEvent)
        {
            // Set Tolerance Range and Convert to UTC
            DateTime oldestNotifiableEventDate = DateTime.UtcNow.AddMinutes(GlobalSettings.NotificationToleranceInMinutes * -1);

            if (deviceEvent.Timestamp < oldestNotifiableEventDate)
                return false;

            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                return !db.DeviceEvents.Where(x => x.UID == deviceEvent.UID &&
                                                   x.DeviceSerialNumber == deviceEvent.DeviceSerialNumber &&
                                                   x.Timestamp > oldestNotifiableEventDate)
                                       .Any(x => x.IsHandled == true);
            };
        }

    }
}