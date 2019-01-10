using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Drive.v3;
using Microsoft.AspNet.Identity;
using PicAggoAPI.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PicAggoAPI.Providers
{
    public class AppFlowMetadata : FlowMetadata
    {
        //ApplicationDbContext _db = new ApplicationDbContext();
        private static readonly IAuthorizationCodeFlow flow =
                      new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                      {
                          ClientSecrets = new ClientSecrets
                          {
                              ClientId = "483997741726-s7thbe7t300jk14ivekc6m55h8r5sqs4.apps.googleusercontent.com",
                              ClientSecret = "Vy3kdmCC9YaPlIwgW4QqCdmq"
                          },
                          Scopes = new[] { DriveService.Scope.Drive },
                          DataStore = new EntityFrameworkDataStore("DefaultConnection")
                      });

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.
            //var user = controller.TempData["userName"].ToString();
            //if (user == "Null")
            //{
            //    user = Guid.NewGuid().ToString();
            //    controller.TempData["userName"] = user;
            //}

            //var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
            //var manager = new UserManager<ApplicationUser>(store);
            //var dummyUser = _db.Users.Where(x => x.UserName == "918817083054").SingleOrDefault();

            return controller.User.Identity.GetUserId();

        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return flow; }
        }

        public override string AuthCallback
        {
            get { return @"/AuthCallback/IndexAsync"; }
        }
    }

    //internal class ForceOfflineGoogleAuthorizationCodeFlow : GoogleAuthorizationCodeFlow
    //{
    //    public ForceOfflineGoogleAuthorizationCodeFlow(GoogleAuthorizationCodeFlow.Initializer initializer) : base(initializer) { }

    //    public override AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
    //    {
    //        return new GoogleAuthorizationCodeRequestUrl(new Uri(AuthorizationServerUrl))
    //        {
    //            ClientId = ClientSecrets.ClientId,
    //            Scope = string.Join(" ", Scopes),
    //            RedirectUri = redirectUri,
    //            AccessType = "offline",
    //            // ApprovalPrompt = "force"
    //        };
    //    }
    //}
}