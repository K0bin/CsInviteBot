using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteamKit2;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace CsInvite.Bot
{
    public class InviteBot: IDisposable
    {
        private ApplicationDbContext db;
        private Steam steam;

        public bool IsRunning
        { get => steam?.IsRunning ?? false; }

        private DateTime lastRepetition = DateTime.Now;

        public InviteBot(IConfiguration configuration)
        {
            var loggerFactory = new LoggerFactory();
            db = new ApplicationDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>(), configuration, loggerFactory);
            steam = new Steam(configuration);
            steam.Bot = this;
            steam.Connect();
        }

        public void Run()
        {
            while (IsRunning)
            {
                steam.Update();
                var didRepetition = RepeatInvites();
                Thread.Sleep(didRepetition ? 500 : 1000);
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }

        public void OnMessageReceived(ulong steamId, string text)
        {
            var users = db.Users
                    .Include(u => u.Friends).ThenInclude(f => f.OtherUser).ThenInclude(u => u.Invites)
                    .Include(u => u.Invites).ThenInclude(i => i.Lobby).ThenInclude(l => l.Members)
                    .Include(u => u.CurrentLobby).ThenInclude(l => l.Members);
            var user = users.FirstOrDefault(u => u.SteamId == steamId);
            if (user == null)
            {
                return;
            }

            var command = text.Trim().ToLower();
            if (command == "cs?")
            {
                LobbyCommand(user);
            }

            if (command == "ja" || command == "nein")
            {
                AnswerCommand(command == "ja", user);
            }
        }

        public void AnswerCommand(bool accept, User user)
        {
            var lastInvite = user.Invites.OrderBy(i => i.Date).LastOrDefault();
            if (lastInvite == null || (DateTime.Now - lastInvite.Date).TotalHours > 2)
            {
                return;
            }
            if (accept)
            {
                if (user.CurrentLobby != null)
                {
                    if (user.CurrentLobby.OwnerId == user.Id)
                    {
                        db.Lobbies.Remove(user.CurrentLobby);
                        steam?.SendMessage(user.SteamId, $"Lobby geschlossen!");
                    }
                    user.CurrentLobby.Members.Remove(user);
                }

                lastInvite.Lobby.Members.Add(user);
                user.CurrentLobby = lastInvite.Lobby;
                lastInvite.Answer = Answer.Accept;
                steam?.SendMessage(user.SteamId, $"Lobby beigetreten! http://k0bin.dnshome.de/lobby/{lastInvite.Lobby.Id}");
            }
            else
            {
                if (user.CurrentLobby == null)
                {
                    steam?.SendMessage(user.SteamId, $"Beitritt abgelehnt!");
                    lastInvite.Answer = Answer.Decline;
                }
                else
                {
                    user.CurrentLobby.Members.Remove(user);
                    user.CurrentLobby = null;
                    lastInvite.Answer = Answer.Decline;
                    steam?.SendMessage(user.SteamId, $"Lobby verlassen!");
                }
            }
            db.SaveChanges();
        }

        public void LobbyCommand(User user)
        {
            var id = StartLobby(user);
            if (id != null)
            {
                steam?.SendMessage(user.SteamId, $"Lobby erstellt! {MakeLobbyLink(id)}");
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
            lobby.Members = new List<User>();
            lobby.Members.Add(user);
            db.Lobbies.Add(lobby);

            user.CurrentLobby = lobby;

            //INVITES      
            var friends = user.Friends.Where(f => {
                    var lastInvite = f.OtherUser.Invites.OrderBy(i => i.Date).LastOrDefault();
                    var minutesSinceLastInvite = (DateTime.Now - (lastInvite?.Date ?? new DateTime())).TotalMinutes;
                    return f.OtherUser.FriendsWithSteamBotIndex != null && minutesSinceLastInvite > 5 && (f.OtherUser.CurrentLobby == null || minutesSinceLastInvite > 120);
                }).OrderBy(f => f.Priority);

            var invitedFriends = 0;
            foreach (var friend in friends)
            {
                var other = friend.OtherUser;
                steam.SendMessage(other.SteamId, "CS???? " + MakeLobbyLink(lobby.Id));
                var invite = new Invite
                {
                    LobbyId = lobby.Id,
                    RecipientId = other.Id,
                    Date = DateTime.Now
                };
                db.Invites.Add(invite);
                other.Invites.Add(invite);
                if (invitedFriends == 4)
                {
                    break;
                }
                invitedFriends++;
            }

            db.SaveChanges();

            return lobby.Id;
        }

        public bool RepeatInvites()
        {
            if ((DateTime.Now - lastRepetition).TotalSeconds < 30)
            {
                return false;
            }
            lastRepetition = DateTime.Now;
            return true;
        }

        private string MakeLobbyLink(string lobbyId)
        {
            return $"http://k0bin.dnshome.de/lobby/{lobbyId}";
        }
    }
}
