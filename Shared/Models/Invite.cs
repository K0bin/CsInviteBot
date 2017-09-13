using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CsInvite.Shared.Models
{
    public class Invite
    {
        [Key, Required, StringLength(36)]
        public string Id { get; set; }

        [StringLength(36), Required]
        public string RecipientId { get; set; }
        public User Recipient { get; set; }

        [StringLength(36), Required]
        public string LobbyId { get; set; }
        public Lobby Lobby
        {
            get; set;
        }
        [Required]
        public DateTime Date { get; set; }
        public Answer Answer { get; set; }
    }

    public enum Answer
    {
        None,
        Accept,
        Decline
    }
}
