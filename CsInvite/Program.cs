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
            var bot = new InviteBot();
            var steam = new Steam();
            steam.Connect();

            var discord = new Discord();
            discord.Connect();

            steam.MessageReceived += bot.OnMessageReceived;
            discord.MessageReceived += bot.OnMessageReceived;
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
