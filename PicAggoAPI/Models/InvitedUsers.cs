using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PicAggoAPI.Models
{
    public class InvitedUsers
    {
        [Key]
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsInvitationAccepted { get; set; }
        public int? EventId { get; set; }
    }
}