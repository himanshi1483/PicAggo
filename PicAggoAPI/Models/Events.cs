using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PicAggoAPI.Models
{
    public class Events
    {
        [Key]
        public int Id { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
        public DateTime? StartTIme { get; set; }
        public DateTime? EndTime { get; set; }
        [Display(Name ="HostId")]
        public string CreatedBy { get; set; }
        public string AlbumLocation { get; set; }
        public string ThumbLocation { get; set; }

        [NotMapped]
        public List<string> InvitedUsers { get; set; }
        [NotMapped]
        public int? GroupId { get; set; }
        [NotMapped]
        public string RemainingDuration { get; set; }
    

    }
}