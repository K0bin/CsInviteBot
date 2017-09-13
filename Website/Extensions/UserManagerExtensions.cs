using CsInvite.Shared.Models;
using CsInvite.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Website.Extensions
{
    public static class UserManagerExtensions
    {
        public static User ToUser(this SteamPlayerSummary player)
        {
            if (player == null)
            {
                return null;
            }
            return new User
            {
                UserName = player.PersonaName,
                AvatarUrl = player.AvatarFull,
                SteamId = player.SteamId
            };
        }
    }
}
