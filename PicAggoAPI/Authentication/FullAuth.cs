using Nexmo.Api;
using Nexmo.Api.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PicAggoAPI.Authentication
{
    public class FullAuth
    {
        public FullAuth()
        {
            var client = new Client(creds: new Credentials("","")
            {
                ApiKey = "NEXMO-API-KEY",
                ApiSecret = "NEXMO-API-SECRET"
            });
        }
    }
}