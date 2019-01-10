using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PicAggoAPI.Models
{
    public class GroupMaster
    {
        [Key]
        public int Id { get; set; }
        public string GroupName { get; set; }
        public bool IsUserCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}