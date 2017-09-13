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
        [Key, Required, StringLength(36)]
        public string Id { get; set; }
        [StringLength(36), Required]
        public string UserId { get; set; }
        public User User { get; set; }
        [StringLength(36), Required]
        public string OtherUserId { get; set; }
        public User OtherUser { get; set; }
        [Required]
        public int Priority { get; set; }
    }
}
