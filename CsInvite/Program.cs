using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CsInvite.Messaging;
using CsInvite.Messaging.Discord;
using CsInvite.Messaging.Steam;
using CsInvite.Bot;

namespace CsInvite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("secret.json");
            var configuration = builder.Build();

            var bot = new InviteBot();
            var steam = new Steam(configuration);
            steam.Connect();

            var discord = new Discord(configuration);
            discord.Connect();

            steam.MessageReceived += bot.OnMessageReceived;
            discord.MessageReceived += bot.OnMessageReceived;
            BuildWebHost(args, configuration).Run();
        }

        public static IWebHost BuildWebHost(string[] args, IConfiguration config) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .Build();
    }
}
