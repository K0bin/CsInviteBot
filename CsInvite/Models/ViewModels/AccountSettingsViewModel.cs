using CsInvite.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Models
{
    public class AccountSettingsViewModel
    {
        public Map PermaBann
        {
            get; set;
        }
        public Map[] Maps
        {
            get; set;
        }
    }
}
