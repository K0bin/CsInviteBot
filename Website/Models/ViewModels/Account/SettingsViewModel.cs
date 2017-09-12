using CsInvite.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Website.Models.ViewModels.Account
{
    public class SettingsViewModel
    {
        [Required]
        public Map PermaBan
        {
            get; set;
        }
    }
}
