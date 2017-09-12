using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CsInvite.SteamBot.Models
{
    public class Friend
    {
        [StringLength(36)]
        public string Id { get; set; }
        [StringLength(36)]
        public string UserId { get; set; }
        [StringLength(36)]
        public string FriendUserId { get; set; }
        public int Priority { get; set; }
        public DateTime LastInvite { get; set; }
    }
}
