using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteamKit2;

namespace CsInvite.Messaging.Steam
{
    public class Chat: Base.Chat
    {
        public SteamID Id
        {
            get; set;
        }
    }
}
