using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Bot.Base
{
    public class FriendEventArgs : EventArgs
    {
        public ulong Id
        {
            get; set;
        }
    }
}
