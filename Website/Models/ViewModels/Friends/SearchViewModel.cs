﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Shared.Models;

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
