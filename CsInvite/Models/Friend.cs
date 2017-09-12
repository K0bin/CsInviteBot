using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Models
{
    public class Friend
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FriendUserId { get; set; }
        public int Priority { get; set; }
        public DateTime LastInvite { get; set; }
    }
}
