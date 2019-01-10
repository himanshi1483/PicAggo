using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using PicAggoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace PicAggoAPI.Providers
{
    public class GDriveRepository
    {
        private static GoogleAuthorizationCodeFlow FlowPromptConsent { get; set; }
        private static string _clientID = "483997741726-s7thbe7t300jk14ivekc6m55h8r5sqs4.apps.googleusercontent.com";
        private static string _clientSecret = "Vy3kdmCC9YaPlIwgW4QqCdmq";
        private ApplicationDbContext _db = new ApplicationDbContext();
        private EntityFrameworkDataStore _tokendb = new EntityFrameworkDataStore("DefaultConnection");
        private ApplicationUserManager _userManager;
        private static GoogleAuthorizationCodeFlow Flow =
             new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
             {
                 ClientSecrets = new ClientSecrets
                 {
                     ClientId = _clientID,
                     ClientSecret = _clientSecret
                 },
                 Scopes = new[] { DriveService.Scope.Drive },
                 DataStore = new EntityFrameworkDataStore("DefaultConnection")
             });
        public static string[] Scopes = { Google.Apis.Drive.v3.DriveService.Scope.Drive };


     
   
       


    }
}