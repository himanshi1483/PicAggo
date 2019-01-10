using System.ComponentModel.DataAnnotations;

namespace PicAggoAPI.Models
{
    public class EventGroupMapping
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public int GroupId { get; set; }
    }
}