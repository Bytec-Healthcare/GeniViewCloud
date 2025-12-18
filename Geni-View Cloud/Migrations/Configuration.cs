namespace GeniView.Cloud.Migrations
{
    using Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;

    internal sealed class Configuration : DbMigrationsConfiguration<GeniView.Cloud.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "GeniView.Cloud.Models.ApplicationDbContext";
        }

        protected override void Seed(GeniView.Cloud.Models.ApplicationDbContext context)
        {
            string[] newRoles = { "Application Admin", "Application User", "Community Admin", "Community Group Admin", "Community User" };
            byte roleID = 0;
            foreach (var roleName in newRoles)
            {
                if (!context.Roles.Any(r => r.Name == roleName))
                {
                    roleID++;
                    var store = new RoleStore<IdentityRole>(context);
                    var manager = new RoleManager<IdentityRole>(store);
                    var role = new IdentityRole { Name = roleName, Id = roleID.ToString() };
                    manager.Create(role);
                }
            }

            string userName = "admin@bytec.com";
            if (!context.Users.Any(u => u.UserName == userName))
            {
                PasswordHasher PasswordHash = new PasswordHasher();
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser
                {
                    UserName = userName,
                    PasswordHash = PasswordHash.HashPassword("admin"),
                    Email = userName,
                    FullName = "Admin",
                    TimeZoneId = "GMT Standard Time",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                manager.Create(user);
                manager.AddToRole(user.Id, "Application Admin");
            }
        }
    }
}
