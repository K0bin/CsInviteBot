using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Shared.Models
{
    public class Friend
    {
        [StringLength(36)]
        public string Id { get; set; }
        public User User { get; set; }
        public User FriendUserId { get; set; }
        public int Priority { get; set; }
        public DateTime LastInvite { get; set; }
    }
}
