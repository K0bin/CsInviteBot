using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using CsInvite.Shared.Models;
using Microsoft.Extensions.Logging;
using CsInvite.Website.Models.Steam;

namespace CsInvite.Website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddLogging();

            services.AddEntityFrameworkSqlite();
            services.AddDbContext<ApplicationDbContext>();

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.User.AllowedUserNameCharacters = null;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddSteam(options => options.ApplicationKey = Configuration["SteamApiKey"]);

            services.AddTransient<Steam>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }


            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Debug);

            app.UseStaticFiles();
            app.UseAuthentication();
            /*app.UseOpenIdAuthentication(new OpenIdAuthenticationOptions
            {
                /*AuthenticationScheme = "Steam",
                DisplayName = "Steam",*
                Authority = new Uri("https://steamcommunity.com/openid/")
            });*/
            app.UseMvcWithDefaultRoute();
            /*app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "lobby",
                    template: "{controller=Lobby}/{action=Index}/{id}");
            });*/
        }
    }
}
