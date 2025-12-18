using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace GeniView.Cloud.Models
{
    public class ApplicationUser : IdentityUser
    {

        public Nullable<long> CommunityID { get; set; }
        public Nullable<long> GroupID { get; set; }
        public string FullName { get; set; }
        public byte[] ProfilePhoto { get; set; }
        public string ImageMimeType { get; set; }
        public string TimeZoneId { get; set; }
        public bool IsNotificationEnable { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("GeniViewCloudIdentityRepository", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}