/*using CsInvite.Bot;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Shared.Models;

namespace CsInvite.Website.Models
{
    public class User: IdentityUser
    {
        [Key, Required, MaxLength(36)]
        public override string Id { get => base.Id; set => base.Id = value; }

        public ulong SteamId
        {
            get; set;
        }
        public Map Permaban
        {
            get; set;
        }
        public int? FriendsWithSteamBotIndex
        {
            get; set;
        }
    }
}
*/