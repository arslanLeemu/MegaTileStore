using Microsoft.Owin;
using Owin;
using MegaTileStore.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

[assembly: OwinStartupAttribute(typeof(MegaTileStore.Startup))]
namespace MegaTileStore
{
    public partial class Startup
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //CreateRoles();
            //CreateUsers();
        }

        private void CreateRoles()
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if(!roleManager.RoleExists(Models.Names.SalesPerson))
            {
                var result = roleManager.Create(new IdentityRole
                {
                    Name = Models.Names.SalesPerson
                });

                if (!result.Succeeded)
                {
                    var errors = "";
                    foreach (var item in result.Errors)
                    {
                        errors += item + '\n';
                    }
                    throw new Exception(errors);
                }
            }

            if(!roleManager.RoleExists(Models.Names.Cashier))
            {
                var result = roleManager.Create(new IdentityRole
                {
                    Name = Models.Names.Cashier
                });

                if (!result.Succeeded)
                {
                    var errors = "";
                    foreach (var item in result.Errors)
                    {
                        errors += item + '\n';
                    }
                    throw new Exception(errors);
                }
            }
        }

        private void CreateUsers()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var salesperson = new ApplicationUser
            {
                UserName = "salesperson",
                Email = "salesperson@leemutech.com"
            };
            var result = userManager.Create(salesperson, Names.DefaultPassword);

            if(result.Succeeded)
            {
                userManager.AddToRole(salesperson.Id, Names.SalesPerson);
            }

            var cashier = new ApplicationUser
            {
                UserName = "cashier",
                Email = "cashier@leemutech.com"
            };

            result = userManager.Create(cashier, Names.DefaultPassword);
            if (result.Succeeded)
            {
                userManager.AddToRole(cashier.Id, Names.Cashier);
            }
        }
    }
}
