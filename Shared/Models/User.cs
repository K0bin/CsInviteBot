using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CsInvite.Shared.Models;

namespace CsInvite.Shared.Models
{
    public class User: IdentityUser
    {
        [Key, Required, MaxLength(36)]
        public override string Id { get => base.Id; set => base.Id = value; }

        [Key, Required]
        public ulong SteamId
        {
            get; set;
        }
        public string AvatarUrl
        {
            get; set;
        }
        public Map Permaban
        {
            get; set;
        }
        public int? FriendsWithSteamBotIndex
        {
            get; set;
        }
        public List<Friend> Friends
        {
            get; set;
        }
        public List<Friend> IsInFriendsListOf
        {
            get; set;
        }
        public string CurrentLobbyId { get; set; }
        public Lobby CurrentLobby
        {
            get; set;
        }

        public List<Lobby> IsOwnerOf { get; set; }
        public List<Invite> Invites { get; set; }
    }
}
