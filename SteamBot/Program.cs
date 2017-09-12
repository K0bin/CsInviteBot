using System;
using CsInvite.SteamBot.Models;
using System.Linq;

namespace CsInvite.SteamBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var db = new BotDbContext();
            var friend = db.Friends.First();
            db.Dispose();
        }
    }
}
