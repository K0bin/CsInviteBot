using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CsInvite.Messaging.Discord
{
    public class Chat: Base.Chat
    {
        internal DiscordChannel Channel
        {
            get; set;
        }
    }
}
