using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PicAggoAPI.Models
{
    public class UserGroupMapping
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public int GroupId { get; set; }
        public bool IsInviteAccepted { get; set; }
        //public virtual ApplicationUser ApplicationUser { get; set; }
        //public virtual GroupsMaster GroupsMaster { get; set; }
    }
}