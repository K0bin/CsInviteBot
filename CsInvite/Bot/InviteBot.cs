using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Messaging.Base;

namespace CsInvite.Bot
{
    public class InviteBot
    {
        public void OnMessageReceived(object sender, MessageEventArgs args)
        {
            Console.WriteLine(args.Message.Text);
            ((MessagingService)sender).SendMessage(args.Message.Chat, "hi");
        }
    }
}
