using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PicAggoAPI.Models
{
    public class EventDetails
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public string SessionActivity { get; set; }
        public DateTime ActivityTime { get; set; }
        public string ActivityBy { get; set; }
        public int PicturesUploaded { get; set; }
    }
}