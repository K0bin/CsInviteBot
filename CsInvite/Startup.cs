using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using CsInvite.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using CsInvite.Messaging.Steam;
using CsInvite.Messaging.Discord;
using CsInvite.Bot;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CsInvite
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

            services.AddEntityFrameworkSqlite();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddSteam(options => options.ApplicationKey = Configuration["SteamApiKey"]);

            //services.ConfigureApplicationCookie(options => options.LoginPath = "/Login");

            var steam = new Steam(Configuration);
            steam.Connect();
            services.AddSingleton(typeof(Steam), steam);
            var discord = new Discord(Configuration);
            discord.Connect();

            var bot = new InviteBot();
            discord.MessageReceived += bot.OnMessageReceived;
            steam.MessageReceived += bot.OnMessageReceived;
            services.AddSingleton(typeof(InviteBot), bot);


            /*services.AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders();*/
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseStaticFiles();
            app.UseAuthentication();
            /*app.UseOpenIdAuthentication(new OpenIdAuthenticationOptions
            {
                /*AuthenticationScheme = "Steam",
                DisplayName = "Steam",*
                Authority = new Uri("https://steamcommunity.com/openid/")
            });*/
            app.UseMvcWithDefaultRoute();
        }
    }
}
