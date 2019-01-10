using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using PicAggoAPI.Models;
using PicAggoAPI.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PicAggoAPI.Controllers
{
    [RoutePrefix("api/Upload")]
    public class UploadController : ApiController
    {
        private static string _clientID = "483997741726-s7thbe7t300jk14ivekc6m55h8r5sqs4.apps.googleusercontent.com";
        private static string _clientSecret = "Vy3kdmCC9YaPlIwgW4QqCdmq";
        private ApplicationDbContext _db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        private readonly IDataStore dataStore = new EntityFrameworkDataStore("DefaultConnection");

        public UploadController()
        {

        }
        public UploadController(IDataStore _datastore, ApplicationUserManager userManager)
        {
            dataStore = _datastore;
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public static object Properties { get; private set; }

        private async Task<UserCredential> GetCredentialForApiAsync()
        {
            string[] scopes = new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile };
            var initializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret,
                },
                Scopes = scopes
            };
            var flow = new GoogleAuthorizationCodeFlow(initializer);

            //var identity = await HttpContext.GetOwinContext().Authentication.GetExternalIdentityAsync(
            //    DefaultAuthenticationTypes.ApplicationCookie);
            var userId = "user1";

            var token = await dataStore.GetAsync<TokenResponse>(userId);
            return new UserCredential(flow, userId, token);
        }


        [HttpPost]
        [Route("createParent")]
        public IHttpActionResult SaveStorage(string auth)
        {

            //Scopes for use with the Google Drive API
            string[] scopes = new string[] { DriveService.Scope.Drive,
                                 DriveService.Scope.DriveFile};

            var user = User.Identity.Name;

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
              new ClientSecrets
              {
                  ClientId = _clientID,
                  ClientSecret = _clientSecret,
              },
               scopes, user, CancellationToken.None, dataStore).Result;

            if (credential.Token.IsExpired(credential.Flow.Clock))
            {
                var refreshed = credential.RefreshTokenAsync(CancellationToken.None).Result;
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "PicAggo",
            });

            var folderId = GoogleDriveService.createDirectory(service, "PicAggo", "PicAggo App Directory", "root");

            var _user = _db.Users.ToList();
            var _us = _user.Where(x => x.UserName == user).FirstOrDefault();
            _us.AppParentFolderId = folderId;
            _db.SaveChanges();

            return Ok(folderId);
        }

        [HttpPost]
        [Route("exchangecode")]
        public string exchangeCode(string code)
        {


            string url = "https://accounts.google.com/o/oauth2/token";
            string postData = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri=urn:ietf:wg:oauth:2.0:oob&grant_type=authorization_code&access_type=offline", code, _clientID, _clientSecret);


            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(url);
            // Set the Method property of the request to POST.

            request.Method = "POST";

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            TokenResponse tmp = JsonConvert.DeserializeObject<TokenResponse>(responseFromServer);

            // Display the content.
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();
            return tmp.RefreshToken;
        }
        //[HttpPost]
        //[Route("uploadImage")]
        //public IHttpActionResult UploadImages(HttpPostedFileBase[] files)
        //{
        //    string[] scopes = new string[] { DriveService.Scope.Drive,
        //                         DriveService.Scope.DriveFile};

        //    var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
        //    {
        //        ClientId = _clientID,
        //        ClientSecret = _clientSecret
        //    }, scopes, Environment.UserName, CancellationToken.None, new FileDataStore("Daimto.GoogleDrive.Auth.Store")).Result;

        //    var service = new DriveService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = "PicAggo",
        //    });

        //    //Ensure model state is valid  
        //    if (ModelState.IsValid)
        //    {   //iterating through multiple file collection   
        //        foreach (HttpPostedFileBase file in files)
        //        {
        //            //Checking file is available to save.  
        //            if (file != null)
        //            {
        //                var InputFileName = System.IO.Path.GetFileName(file.FileName);

        //                GoogleDriveService.uploadFile(service, file, " ");
        //            }

        //        }
        //    }
        //    return Ok();
        //}


        public Task<HttpResponseMessage> Post()
        {

            List<string> savedFilePath = new List<string>();
            // Check if the request contains multipart/form-data
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            //Get the path of folder where we want to upload all files.
            string rootPath = HttpContext.Current.Server.MapPath("~/documents");
            var provider = new MultipartFileStreamProvider(rootPath);
            // Read the form data.
            //If any error(Cancelled or any fault) occurred during file read , return internal server error
            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    foreach (MultipartFileData dataitem in provider.FileData)
                    {
                        try
                        {
                            //Replace / from file name
                            string name = dataitem.Headers.ContentDisposition.FileName.Replace("\"", "");
                            //Create New file name using GUID to prevent duplicate file name
                            string newFileName = Guid.NewGuid() + System.IO.Path.GetExtension(name);
                            //Move file from current location to target folder.
                            System.IO.File.Move(dataitem.LocalFileName, System.IO.Path.Combine(rootPath, newFileName));


                        }
                        catch (Exception ex)
                        {
                            string message = ex.Message;
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.Created, savedFilePath);
                });
            return task;
        }
    }
}
