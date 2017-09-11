using CsInvite.Bot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Models.ViewModels
{
    public class AccountSettingsViewModel
    {
        [Required]
        public Map PermaBan
        {
            get; set;
        }
    }
}
