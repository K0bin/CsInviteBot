using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Messaging.Base
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
    }
}
