using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Shared.Models
{
    public class Friend
    {
        [StringLength(36)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string OtherUserId { get; set; }
        public User OtherUser { get; set; }
        public int Priority { get; set; }
        public DateTime LastInvite { get; set; }
    }
}
