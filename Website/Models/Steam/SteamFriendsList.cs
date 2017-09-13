using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsInvite.Website.Models.Steam
{
    public class SteamFriend
    {
        public ulong SteamId { get; set; }
        public string Relationship { get; set; }
        public long FriendSince { get; set; }
    }
    public class SteamFriendsList
    {
        public List<SteamFriend> Friends { get; set; }
    }

    public class SteamFriendsRootObject
    {
        public SteamFriendsList FriendsList { get; set; }
    }
}
