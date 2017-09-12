using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Bot.Base
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message
        {
            get; set;
        }
    }
}
