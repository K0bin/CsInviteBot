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

            using (var bot = new InviteBot(configuration))
            {
                bot.Run();
            }
        }
    }
}
