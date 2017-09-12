using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Shared.Models;

namespace CsInvite.Bot.Base
{
    public class Message
    {
        public string Text
        {
            get; set;
        }

        public Chat Chat
        {
            get; set;
        }

        public SupportedMessagingService Service
        {
            get; set;
        }
           
    }
}
