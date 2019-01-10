using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PicAggoAPI.Models
{
    public class StoredResponse
    {
        private string _refreshToken;
        private string _access_token;
        private string _Issued;
        private string _expires_in;
        private string _UserId;
        public StoredResponse()
        {

        }
        public StoredResponse(string userId)
        {
            this._UserId = UserID;
        }

        [Key]
        public Guid Id { get; set; }
        public string access_token { get; private set; }
        public string token_type { get; set; }
        public long? expires_in { get; private set; }
        public string refresh_token { get; private set; }
        public string Issued { get; private set; }
        public string UserID { get; set; }
    }
}