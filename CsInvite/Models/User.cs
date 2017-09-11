using CsInvite.Bot;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Models
{
    public class User: IdentityUser
    {
        public string SteamId
        {
            get; set;
        }
        public Map Permaban
        {
            get; set;
        }
    }
}
