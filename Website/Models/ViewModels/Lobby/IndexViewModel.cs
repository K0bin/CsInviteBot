using CsInvite.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Website.Models.ViewModels.Lobby
{
    public class IndexViewModel
    {
        public User Owner { get; set; }
        public List<User> Members { get; set; }
        public List<Invite> Invites { get; set; }
        public DateTime Started { get; set; }
    }
}
