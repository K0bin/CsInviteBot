using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Website.Models.ViewModels.Friends
{
    public class SearchViewModel
    {
        public string Query
        { get; set; }

        public List<User> Users
        { get; set; }
    }
}
