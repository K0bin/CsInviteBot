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

        public List<Job> jobs = new List<Job>();

        public InviteBot(IConfiguration configuration)
        {
            var loggerFactory = new LoggerFactory();
            db = new ApplicationDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>(), configuration, loggerFactory);
            steam = new Steam(configuration);
            steam.Bot = this;
            steam.Connect();

            jobs.Add(new Job
            {
                Duration = new TimeSpan(0, 30, 0),
                Action = this.CleanDatabase
            });
            jobs.Add(new Job
            {
                Duration = new TimeSpan(0, 5, 0),
                Action = this.AcceptSteamFriends
            });
            jobs.Add(new Job
            {
                Duration = new TimeSpan(0, 1, 0),
                Action = this.RefreshInvites
            });
            jobs.Add(new Job
            {
                Duration = new TimeSpan(0, 3, 0),
                Action = this.InviteToLobby
            });
            jobs.Add(new Job
            {
                Duration = new TimeSpan(0, 3, 0),
                Action = this.SyncFriendsList
            });
        }

        public void Run()
        {
            while (IsRunning)
            {
                ExecuteJobs();
                steam.Update();
                Thread.Sleep(200);
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
                return;
            }

            if (command == "ja" || command == "nein")
            {
                AnswerCommand(command == "ja", user);
                return;
            }

            steam.SendMessage(steamId, "Ich habe dich nicht verstanden.\n\nWenn du eine Lobby eröffnen möchtest, schreibe \"cs?\".\n\nWenn du in eine Lobby eingeladen wurdest, schreibe entweder \"ja\" oder \"nein\".");
        }

        public void AnswerCommand(bool accept, User user)
        {
            var lastInvite = user.Invites.OrderBy(i => i.Date).LastOrDefault();
            if (user.CurrentLobby == null && (lastInvite == null || (DateTime.Now - lastInvite.Date).TotalHours > 2))
            {
                return;
            }
            if (accept)
            {
                if (lastInvite != null)
                {
                    if (user.CurrentLobby != null)
                    {
                        if (user.CurrentLobby.OwnerId == user.Id)
                        {
                            db.Lobbies.Remove(user.CurrentLobby);
                            steam?.SendMessage(user.SteamId, $"Lobby geschlossen!");
                        }
                        else
                        {
                            user.CurrentLobby.LastModified = DateTime.Now;
                            user.CurrentLobby.Members.Remove(user);
                            user.CurrentLobby = null;
                        }
                    }
                    lastInvite.Lobby.Members.Add(user);
                    lastInvite.Lobby.LastModified = DateTime.Now;
                    user.CurrentLobby = lastInvite.Lobby;
                    steam?.SendMessage(user.SteamId, $"Lobby beigetreten! http://k0bin.dnshome.de/lobby/{lastInvite.Lobby.Id}");
                    lastInvite.Answer = Answer.Accept;
                }
            }
            else
            {
                if (user.CurrentLobby == null)
                {
                    steam?.SendMessage(user.SteamId, $"Beitritt abgelehnt!");
                    if (lastInvite != null)
                    {
                        lastInvite.Answer = Answer.Decline;
                    }
                }
                else
                {
                    if (user.CurrentLobby.OwnerId == user.Id)
                    {
                        db.Lobbies.Remove(user.CurrentLobby);
                        steam?.SendMessage(user.SteamId, $"Lobby geschlossen!");
                    }
                    else
                    {
                        user.CurrentLobby.Members.Remove(user);
                        user.CurrentLobby = null;
                        user.CurrentLobby.LastModified = DateTime.Now;
                    }
                    if (lastInvite != null)
                    {
                        lastInvite.Answer = Answer.Decline;
                        lastInvite.Lobby.LastModified = DateTime.Now;
                    }
                    steam?.SendMessage(user.SteamId, $"Lobby verlassen!");
                }
            }
            db.SaveChanges();
        }

        public void LobbyCommand(User user)
        {
            StartLobby(user);
        }

        public void StartLobby(User user)
        {
            var existingLobby = db.Lobbies.FirstOrDefault(l => l.Owner == user);
            if (existingLobby != null)
            {
                steam?.SendMessage(user.SteamId, $"Lobby existiert bereits: {MakeLobbyLink(existingLobby.Id)}");
                return;
            }
            var lobby = new Lobby();
            lobby.Created = DateTime.Now;
            lobby.LastModified = DateTime.Now;
            lobby.Owner = user;
            lobby.Members = new List<User>();
            lobby.Members.Add(user);
            lobby.Invites = new List<Invite>();
            db.Lobbies.Add(lobby);

            user.CurrentLobby = lobby;
            steam?.SendMessage(user.SteamId, $"Lobby erstellt: {MakeLobbyLink(lobby.Id)}");

            InviteToLobby(lobby);
            db.SaveChanges();
        }

        public int ExecuteJobs()
        {
            var jobsCount = 0;
            foreach (var job in jobs)
            {
                if (job.ExecuteIfNecessary())
                {
                    jobsCount++;
                }
            }
            return jobsCount;
        }

        public void RefreshInvites()
        {
            var lobbies = db.Lobbies
                .Include(l => l.Invites).ThenInclude(i => i.Recipient).ThenInclude(u => u.CurrentLobby)
                .Include(l => l.Members).ThenInclude(u => u.Invites)
                .Where(l => l.Invites != null && l.Invites.Count > 0);

            foreach (var lobby in lobbies)
            {
                var membersCount = lobby.Members.Count;
                var openInvitesCount = lobby.Invites.Count(i =>
                {
                    var lastInvite = i.Recipient.Invites.OrderBy(_i => _i.Date).LastOrDefault();
                    var minutesSinceLastInvite = (DateTime.Now - (lastInvite?.Date ?? new DateTime())).TotalMinutes;
                    var minutesSinceThisInvite = (DateTime.Now - i.Date).TotalMinutes;
                    var minutesSinceRefreshed = (DateTime.Now - i.LastRefreshed).TotalMinutes;
                    return i.Answer == Answer.None && minutesSinceLastInvite < 15 && minutesSinceRefreshed > 3 && i.Recipient.IsOnline && (i.Recipient.CurrentLobby == null || minutesSinceLastInvite > 45);
                });
                var slotsLeft = 5 - membersCount - openInvitesCount;
                if (slotsLeft <= 0)
                {
                    return;
                }

                var invitedFriends = lobby.Invites.Where(i =>
                {
                    var lastInvite = i.Recipient.Invites.OrderBy(_i => _i.Date).LastOrDefault();
                    var minutesSinceLastInvite = (DateTime.Now - (lastInvite?.Date ?? new DateTime())).TotalMinutes;
                    var minutesSinceThisInvite = (DateTime.Now - i.Date).TotalMinutes;
                    return i.Answer == Answer.None
                    && minutesSinceThisInvite > 5 && minutesSinceThisInvite < 20 && i.Recipient.IsOnline && (i.Recipient.CurrentLobbyId == null || minutesSinceLastInvite > 45);
                });
                foreach (var friend in invitedFriends)
                {
                    friend.LastRefreshed = DateTime.Now;
                    steam.SendMessage(friend.Recipient.SteamId, AskForCs(lobby));
                    steam.SendMessage(lobby.Owner.SteamId, friend.Recipient.UserName + " wurde erneut gefragt.");
                }
            }
        }

        public void InviteToLobby()
        {
            var lobbies = db.Lobbies.Include(l => l.Owner).ThenInclude(u => u.Friends).ThenInclude(f => f.OtherUser).ThenInclude(l => l.Invites).ThenInclude(i => i.Recipient)
                .Include(l => l.Invites).ThenInclude(i => i.Recipient).ThenInclude(u => u.CurrentLobby)
                .Include(l => l.Members).ThenInclude(u => u.Invites)
                .Where(l => l.Members.Count < 5);

            foreach (var lobby in lobbies)
            {
                InviteToLobby(lobby);
            }
        }

        public void InviteToLobby(Lobby lobby)
        {
            var membersCount = lobby.Members.Count;
            var openInvitesCount = lobby.Invites.Count(i =>
            {
                var lastInvite = i.Recipient.Invites.OrderBy(_i => _i.Date).LastOrDefault();
                var minutesSinceLastInvite = (DateTime.Now - (lastInvite?.Date ?? new DateTime())).TotalMinutes;
                var minutesSinceThisInvite = (DateTime.Now - i.Date).TotalMinutes;
                return i.Answer == Answer.None && minutesSinceThisInvite < 10 && i.Recipient.IsOnline && (i.Recipient.CurrentLobby == null || minutesSinceLastInvite > 45);
            });
            var slotsLeft = 5 - membersCount - openInvitesCount;
            if (slotsLeft <= 0)
            {
                return;
            }

            var friends = lobby.Owner.Friends.Where(f =>
            {
                var lastInvite = f.OtherUser.Invites.OrderBy(i => i.Date).LastOrDefault();
                var minutesSinceLastInvite = (DateTime.Now - (lastInvite?.Date ?? new DateTime())).TotalMinutes;
                return f.OtherUser.FriendsWithSteamBotIndex != null && f.OtherUser.IsOnline && lastInvite?.LobbyId != lobby.Id && f.OtherUser.CurrentLobbyId != lobby.Id && (f.OtherUser.CurrentLobby == null || minutesSinceLastInvite > 120);
            }).OrderBy(f => f.Priority).Take(slotsLeft);

            foreach (var friend in friends)
            {
                var other = friend.OtherUser;
                steam.SendMessage(other.SteamId, AskForCs(lobby));
                steam.SendMessage(lobby.Owner.SteamId, friend.OtherUser.UserName + " wurde eingeladen");
                var invite = new Invite
                {
                    LobbyId = lobby.Id,
                    RecipientId = other.Id,
                    Date = DateTime.Now,
                    LastRefreshed = DateTime.Now
                };
                db.Invites.Add(invite);
                other.Invites.Add(invite);
            }
        }

        public void SyncFriendsList()
        {
            var steamFriends = steam.GetFriends();
            var ids = steamFriends.Select(s => s.SteamId);

            var users = db.Users.Where(u => ids.Contains(u.SteamId) || (u.FriendsWithSteamBotIndex != null && !ids.Contains(u.SteamId)));
            foreach (var user in users)
            {
                var steamUser = steamFriends.FirstOrDefault(su => su.SteamId == user.SteamId);
                var isFriendsWithBot = steamUser != null;
                if (user.FriendsWithSteamBotIndex == null && isFriendsWithBot)
                {
                    user.FriendsWithSteamBotIndex = 0;
                }
                else if (user.FriendsWithSteamBotIndex != null && !isFriendsWithBot)
                {
                    user.FriendsWithSteamBotIndex = null;
                }
                if (steamUser != null)
                {
                    user.UserName = steamUser.Persona;
                    user.IsOnline = steamUser.State != EPersonaState.Offline;
                }
            }
            db.SaveChanges();
        }

        public void AcceptSteamFriends()
        {
            //TODO
        }

        public void CleanDatabase()
        {
            var lobbies = db.Lobbies.Include(l => l.Invites);
            foreach (var lobby in lobbies)
            {
                if ((DateTime.Now - lobby.LastModified).TotalMinutes > 120)
                {
                    db.Lobbies.Remove(lobby);
                    continue;
                }

                var staleInvites = lobby.Invites.Where(i => i.Answer == Answer.None && (DateTime.Now - i.Date).TotalMinutes > 15);
                foreach (var invite in staleInvites)
                {
                    invite.Answer = Answer.Decline;
                }
            }
            db.SaveChanges();
        }

        private string MakeLobbyLink(string lobbyId)
        {
            return $"http://k0bin.dnshome.de/lobby/{lobbyId}";
        }

        private string AskForCs(Lobby lobby)
        {
            return $"{lobby.Owner.UserName} möchte CS Spielen.\n\nDie Lobby ist: {MakeLobbyLink(lobby.Id)}.\n\nAntworte bitte mit \"ja\" oder \"nein\".";
        }
    }
}
