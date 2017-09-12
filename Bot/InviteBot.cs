using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Bot.Base;
using CsInvite.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace CsInvite.Bot
{
    public class InviteBot: IDisposable
    {
        private ApplicationDbContext db;

        public bool IsRunning
        { get; private set; } = true;

        public InviteBot(IConfiguration configuration)
        {
            db = new ApplicationDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>(), configuration);
        }

        public void Dispose()
        {
            db.Dispose();
        }

        public void OnMessageReceived(object sender, MessageEventArgs args)
        {
            var message = args.Message;
            var service = ((MessagingService)sender);
            Console.WriteLine(args.Message.Text);
            ((MessagingService)sender).SendMessage(args.Message.Chat, "hi");

            User user = null;
            if (message.Service == SupportedMessagingService.Steam)
            {
                var steamUser = ((Steam.Chat)message.Chat);
                user = db.Users.FirstOrDefault(u => u.SteamId == steamUser.Id);
            }

            if (user == null)
            {
                return;
            }

            var command = args.Message.Text.Trim().ToLower();
            if (command == "cs?")
            {
                LobbyCommand(service, message.Chat, user);
            }
        }

        public void LobbyCommand(MessagingService service, Chat chat, User user)
        {
            var id = StartLobby(user);
            if (id != null)
            {
                service.SendMessage(chat, $"Lobby erstellt! http://k0bin.dnshome.de/lobby/{id}");
            }
        }

        public string StartLobby(User user)
        {
            if (db.Lobbies.Any(l => l.Owner == user))
            {
                return null;
            }
            var lobby = new Lobby();
            lobby.Created = DateTime.Now;
            lobby.LastModified = DateTime.Now;
            lobby.Owner = user;
            db.Lobbies.Add(lobby);
            return lobby.Id;
        }
    }
}
