using System;
using CsInvite.Shared.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading;

namespace CsInvite.Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("secret.json");
            var configuration = builder.Build();

            var steam = new Steam.Steam(configuration);
            steam.Connect();

            using (var bot = new InviteBot(configuration))
            {
                steam.MessageReceived += bot.OnMessageReceived;

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
