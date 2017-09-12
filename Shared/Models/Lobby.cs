using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CsInvite.Shared.Models
{
    public class Lobby
    {
        [StringLength(36)]
        public string Id { get; set; }
        public User Owner { get; set; }
        public List<User> UserIds { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }
}
