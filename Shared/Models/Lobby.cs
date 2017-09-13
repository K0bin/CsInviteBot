using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CsInvite.Shared.Models
{
    public class Lobby
    {
        [Key, Required, StringLength(36)]
        public string Id { get; set; }
        [StringLength(36)]
        public string OwnerId { get; set; }
        public User Owner { get; set; }
        public List<User> Members { get; set; }
        public List<Invite> Invites { get; set; }
        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime LastModified { get; set; }

        public List<User> IsLastInviteOf { get; set; }
    }
}
