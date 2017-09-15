using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using SteamKit2;
using Microsoft.Extensions.Configuration;

namespace CsInvite.Bot
{
    public class Steam
    {
        private SteamClient steamClient = new SteamClient();
        private CallbackManager manager;
        private SteamUser user;
        private SteamFriends friends;

        private Thread steamThread;

        public bool IsRunning { get; private set; } = false;

        private IConfiguration configuration;

        public InviteBot Bot
        {
            get; set;
        }

        public Steam(IConfiguration configuration)
        {
            this.configuration = configuration;

            manager = new CallbackManager(steamClient);
            user = steamClient.GetHandler<SteamUser>();
            friends = steamClient.GetHandler<SteamFriends>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnChatMessage);
            manager.Subscribe<SteamGameCoordinator.MessageCallback>(OnGameCoordinator);
            manager.Subscribe<SteamFriends.FriendAddedCallback>(OnFriendsListChanged);
            manager.Subscribe<SteamFriends.IgnoreFriendCallback>(OnFriendsListChanged);

        }

        public void Connect()
        {
            steamClient.Connect();
            IsRunning = true;
        }

        public void Update()
        {
            if (IsRunning)
            {
                manager.RunCallbacks();
            }
        }

        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            user.LogOn(new SteamUser.LogOnDetails()
            {
                Username = configuration["SteamUsername"],
                Password = configuration["SteamPassword"]
            });
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            IsRunning = false;
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 5 for how SteamGuard can be handled

                    Console.WriteLine("Unable to logon to Steam: This account is SteamGuard protected.");

                    IsRunning = false;
                    return;
                }

                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                IsRunning = false;
                return;
            }
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {

        }

        private void SyncFriends()
        {
            var friends = GetFriends();
            foreach (var friend in friends)
            {

            }
        }

        private void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType == EChatEntryType.ChatMsg)
            {
                Bot?.OnMessageReceived(callback.Sender.ConvertToUInt64(), callback.Message);
            }
        }

        private void OnGameCoordinator(SteamGameCoordinator.MessageCallback callback)
        {

        }

        private void OnFriendsListChanged(SteamFriends.FriendAddedCallback callback)
        {

        }

        private void OnFriendsListChanged(SteamFriends.IgnoreFriendCallback callback)
        {

        }

        public void SendMessage(ulong user, string message)
        {
            var steamUser = new SteamID();
            steamUser.SetFromUInt64(user);
            friends.SendChatMessage(steamUser, EChatEntryType.ChatMsg, message);
        }

        private void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            friends.SetPersonaState(EPersonaState.Online);
            friends.SetPersonaName("CSInviteBot");
        }

        public void AddFriend(ulong steamId)
        {
            var id = new SteamID();
            id.SetFromUInt64(steamId);
            friends.AddFriend(id);
        }

        public List<SteamFriend> GetFriends()
        {
            var steamFriends = new List<SteamFriend>();
            for (int i = 0; i < this.friends.GetFriendCount(); i++)
            {
                var id = this.friends.GetFriendByIndex(i).ConvertToUInt64();
                steamFriends.Add(new SteamFriend
                {
                    SteamId = id,
                    Persona = this.friends.GetFriendPersonaName(id),
                    State = this.friends.GetFriendPersonaState(id)
                });
            }
            return steamFriends;
        }

        public class SteamFriend
        {
            public ulong SteamId { get; set; }
            public EPersonaState State { get; set; }
            public string Persona { get; set; }
            public string AvatarUrl { get; set; }
        }
    }
}
