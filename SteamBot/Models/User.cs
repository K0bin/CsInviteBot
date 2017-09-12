using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using CsInvite.Shared.Models;

namespace CsInvite.SteamBot.Models
{
    [Table("AspNetUsers")]
    class User
    {
        [Key, Required, MaxLength(36)]
        public string Id { get; set; }

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
