using GeniView.Cloud.Repository;
using GeniView.Data.Hardware.Event;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;

namespace GeniView.Cloud.Models
{
    public class MailHelper
    {
        private MailServer model;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public MailHelper()
        {
            using (GeniViewCloudDataRepository db = new GeniViewCloudDataRepository())
            {
                model = db.MailServer.FirstOrDefault();
            }
        }
        public async Task SendMailAsync(string destination, string subject, string body)
        {
            if (model != null)
            {
                var client = new SmtpClient
                {
                    Host = model.Host,
                    Port = model.Port,
                    DeliveryMethod = model.DeliveryMethod,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(model.User, model.Password),
                    EnableSsl = model.EnableSsl,
                };

                var @from = new MailAddress(model.User);
                var to = new MailAddress(destination);


                var mail = new MailMessage(@from, to)
                {
                    Subject = subject,
                    Body = ViewRenderer.RenderView("~/Views/MessageBodies/SimpleContainer.cshtml", body),
                    IsBodyHtml = true,
                    ReplyTo = model.ReplyTo != null ? new MailAddress(model.ReplyTo) : @from,
                };

                try
                {
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                }

            }
        }

        public async Task SendMailAsync(string fullName, string email, MessageEnumeration mEnum, string callbackUrl)
        {
            if (model != null)
            {
                MessageViewModel mvm = new MessageViewModel { FullName = fullName, CallbackUrl = callbackUrl };

                var client = new SmtpClient
                {
                    Host = model.Host,
                    Port = model.Port,
                    DeliveryMethod = model.DeliveryMethod,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(model.User, model.Password),
                    EnableSsl = model.EnableSsl,
                };

                var @from = new MailAddress(model.User);
                var to = new MailAddress(email);

                string body = "";
                switch (mEnum)
                {
                    case MessageEnumeration.ResetPassword:
                        mvm.Subject = "Geni-View Cloud Reset Password Request";
                        body = ViewRenderer.RenderView("~/Views/MessageBodies/ResetPassword.cshtml", mvm);
                        break;
                    case MessageEnumeration.ConfirmEmail:
                        mvm.Subject = "Geni-View Cloud Email Confirmation";
                        body = ViewRenderer.RenderView("~/Views/MessageBodies/ConfirmEmail.cshtml", mvm);
                        break;
                    case MessageEnumeration.VerifyMailServer:
                        mvm.Subject = "Geni-View Cloud Mail Server Configuration";
                        body = ViewRenderer.RenderView("~/Views/MessageBodies/VerifyMailServer.cshtml", mvm);
                        break;
                }

                var mail = new MailMessage(@from, to)
                {
                    Subject = mvm.Subject,
                    Body = body,
                    IsBodyHtml = true,
                    ReplyTo = model.ReplyTo != null ? new MailAddress(model.ReplyTo) : @from,
                };

                try
                {
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                    throw;
                }

            }
        }

        public async Task SendMailAsync(string destination, DeviceEvent deviceEvent)
        {
            if (model != null)
            {
                var client = new SmtpClient
                {
                    Host = model.Host,
                    Port = model.Port,
                    DeliveryMethod = model.DeliveryMethod,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(model.User, model.Password),
                    EnableSsl = model.EnableSsl,
                };

                var @from = new MailAddress(model.User);
                var to = new MailAddress(destination);
                try
                {
                    #region Prepare Message Body
                    // NOTE : HostingEnvironment.MapPath can be used also in WCF and MVC ...
                    string mBody = "";
                    mBody = System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~/Views/MessageBodies/DeviceEvent.html"));
                    mBody = mBody.Replace("#mSubject", "Geni - View Cloud Device Notification");
                    mBody = mBody.Replace("#mDeviceSerialNumber", deviceEvent.DeviceSerialNumber);
                    mBody = mBody.Replace("#mEventType", deviceEvent.EventTypeText);
                    mBody = mBody.Replace("#mDescription", deviceEvent.Description);
                    mBody = mBody.Replace("#mSource", deviceEvent.SourceText);
                    mBody = mBody.Replace("#mTimestamp", deviceEvent.Timestamp.ToString());
                    mBody = mBody.Replace("#mDateTimeNow", DateTime.UtcNow.Year.ToString());
                    #endregion

                    var mail = new MailMessage(@from, to)
                    {
                        Subject = "Geni-View Cloud Device Notification",
                        Body = mBody,
                        IsBodyHtml = true,
                        ReplyTo = model.ReplyTo != null ? new MailAddress(model.ReplyTo) : @from,
                    };
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    _logger.Error("Geni-View Cloud encountered an error. More information about error in details row.", ex);
                }
            }
            else
            {
                _logger.Warn("Cannot send notifications, because mail server configuration not completed.");
            }
        }

        public bool IsMailServerConfigured()
        {
            if (model != null)
                return true;
            return false;
        }
    }
}