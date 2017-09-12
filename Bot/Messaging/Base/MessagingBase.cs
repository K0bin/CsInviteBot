using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Bot.Base
{
    public delegate void MessageReceivedHandler(object sender, MessageEventArgs eventArgs);
    public delegate void FriendAddedHandler(object sender, FriendEventArgs eventArgs);
    public delegate void FriendIgnoredHandler(object sender, FriendEventArgs eventArgs);

    public abstract class MessagingService
    {
        public event MessageReceivedHandler MessageReceived;
        public event FriendAddedHandler FriendAdded;
        public void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs()
            {
                Message = message
            });
        }
        public void OnFriendAdded(ulong id)
        {
            FriendAdded?.Invoke(this, new FriendEventArgs
            {
                Id = id
            });
        }

        public abstract void SendMessage(Chat chat, string message);
    }
}
