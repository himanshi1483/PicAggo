using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;

namespace PicAggoAPI.Models
{
    public class LogMetadata
    {
        [Key]
        public int Id { get; set; }
        public string RequestContentType { get; set; }
        public string RequestUri { get; set; }
        public string RequestMethod { get; set; }
        public DateTime? RequestTimestamp { get; set; }
        public string ResponseContentType { get; set; }
        public string ResponseStatusCode { get; set; }
        public DateTime? ResponseTimestamp { get; set; }
    }

    public class ResponseModel
    {
        public string Message { set; get; }
        public HttpStatusCode Status { set; get; }
        public object Data { set; get; }
        public string  AccessToken { get; set; }
    }
}