using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using SteamKit2;
using CsInvite.Messaging.Base;
using Microsoft.Extensions.Configuration;

namespace CsInvite.Messaging.Steam
{
    public class Steam: MessagingService
    {
        private SteamClient steamClient = new SteamClient();
        private CallbackManager manager;
        private SteamUser user;
        private SteamFriends friends;

        private Thread steamThread;

        private bool isRunning = false;

        private IConfiguration configuration;

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
            manager.Subscribe<SteamFriends.ChatMsgCallback>(OnChatMessage);
            manager.Subscribe<SteamFriends.FriendMsgEchoCallback>(OnChatMessage);
            manager.Subscribe<SteamGameCoordinator.MessageCallback>(OnGameCoordinator);

            steamThread = new Thread(this.ConnectOnThread);
        }

        public void Connect()
        {
            steamThread.Start();
        }

        private void ConnectOnThread()
        {
            steamClient.Connect();
            isRunning = true;

            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
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
            isRunning = false;
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

                    isRunning = false;
                    return;
                }

                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                isRunning = false;
                return;
            }
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {

        }

        private void OnChatMessage(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType == EChatEntryType.ChatMsg)
            {
                OnMessageReceived(new Message()
                {
                    Text = callback.Message,
                    Chat = new Chat()
                    {
                        Id = callback.Sender
                    }
                });
            }
        }

        private void OnChatMessage(SteamFriends.FriendMsgEchoCallback callback)
        {
            if (callback.EntryType == EChatEntryType.ChatMsg)
            {
                OnMessageReceived(new Message());
            }
        }

        private void OnChatMessage(SteamFriends.ChatMsgCallback callback)
        {
            if (callback.ChatMsgType == EChatEntryType.ChatMsg)
            {
                OnMessageReceived(new Message()
                {
                    Text = callback.Message.Trim()
                });
            }
        }

        private void OnGameCoordinator(SteamGameCoordinator.MessageCallback callback)
        {

        }

        public override void SendMessage(Base.Chat chat, string message)
        {
            var sChat = chat as Chat;
            if (sChat == null)
            {
                return;
            }
            friends.SendChatMessage(sChat.Id, EChatEntryType.ChatMsg, message);
        }

        private void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            friends.SetPersonaState(EPersonaState.Online);
        }

        public void AddFriend(ulong steamId)
        {
            var id = new SteamID();
            id.SetFromUInt64(steamId);
            friends.AddFriend(id);
        }

        public List<ulong> GetFriends()
        {
            var friends = new List<ulong>();
            for (int i = 0; i < this.friends.GetFriendCount(); i++)
            {
                friends.Add(this.friends.GetFriendByIndex(i).ConvertToUInt64());
            }
            return friends;
        }
    }
}
