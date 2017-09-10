using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Messaging.Base
{
    public delegate void MessageReceivedHandler(object sender, MessageEventArgs eventArgs);

    public abstract class MessagingService
    {
        public event MessageReceivedHandler MessageReceived;
        public void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs()
            {
                Message = message
            });
        }

        public abstract void SendMessage(Chat chat, string message);
    }
}
