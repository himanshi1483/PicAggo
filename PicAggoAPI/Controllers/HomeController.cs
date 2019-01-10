using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Drive.v3;
using Google.Apis.Logging;
using Google.Apis.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexmo.Api;
using PicAggoAPI.Models;
using PicAggoAPI.Providers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Google.Apis.Drive.v2.DriveService;

namespace PicAggoAPI.Controllers
{
    // [Route("api/Verification")]

    public class HomeController : Controller
    {

        private static GoogleAuthorizationCodeFlow FlowPromptConsent { get; set; }
        private static string _clientID = "483997741726-s7thbe7t300jk14ivekc6m55h8r5sqs4.apps.googleusercontent.com";
        private static string _clientSecret = "Vy3kdmCC9YaPlIwgW4QqCdmq";
        public Client Client { get; set; }
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

        public HomeController()
        {
            Client = new Client(creds: new Nexmo.Api.Request.Credentials("f40d3101", "MgI6GsGyCz80tTNr")
            {

                ApiKey = "f40d3101",
                ApiSecret = "MgI6GsGyCz80tTNr"
            });
        }

        public HomeController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
            // _db = db;
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

        public string _userName { get; private set; }

        public ActionResult Index()
        {
            return View();
        }

        //Add google credentials, update email address and share folder
        [AcceptVerbs(HttpVerbs.Post)]
        [Route("saveStorage")]
        [Authorize]
        public JObject SaveStorage(string authCode, string email)
        {
            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new EntityFrameworkDataStore("DefaultConnection")
            });

            //var t = apiCodeFlow.ExchangeCodeForTokenAsync(User.Identity.GetUserId(), authCode, "https://picaggo.azurewebsites.net", CancellationToken.None).Result;

            TokenResponse token = new TokenResponse();
            //token = t;



            var content = new FormUrlEncodedContent(new Dictionary<string, string>
             {
                 {"grant_type", "authorization_code"},
                 {"code", authCode},
                 {"redirect_uri",  "https://picaggo.azurewebsites.net"},
                 {"client_id", _clientID},
                 {"client_secret", _clientSecret},
                 {"prompt", "consent"},
                 { "access_type","offline"}
             });
            GoogleAccessToken googletoken = new GoogleAccessToken();
            // TokenResponse token = new TokenResponse();
            string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
            TokenResponse token1 = new TokenResponse();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var response = client.PostAsync(tokenEndpoint, content);
                var responseContent = response.Result.Content.ReadAsStringAsync();
                googletoken = JsonConvert.DeserializeObject<GoogleAccessToken>(responseContent.Result);
                token = JsonConvert.DeserializeObject<TokenResponse>(responseContent.Result);
                token.AccessToken = googletoken.access_token;
                token.RefreshToken = googletoken.refresh_token;
                token.Issued = DateTime.Now;
                token.IssuedUtc = DateTime.UtcNow;              
                token.ExpiresInSeconds = googletoken.expires_in;
                if (!response.Result.IsSuccessStatusCode)
                {
                    var result = new ResponseModel
                    {
                        Message = "Auth Code Expired",
                        Status = HttpStatusCode.BadRequest,
                        Data = null

                    };
                    var d = JsonConvert.SerializeObject(result);
                    var s = JObject.Parse(d);
                    return s;
                    //return "Auth Code Expired";
                }
            }
            try
            {
                var _user = UserManager.Users.ToList();
                var _userdata = new ApplicationUser();
                var id = User.Identity.GetUserId();
                _userdata = _user.Where(x => x.Id == id).FirstOrDefault();

                //update email address
                _userdata.Email = email;
                _userdata.EmailConfirmed = true;
                UserManager.Update(_userdata);
                _db.SaveChanges();


                string[] scopes = new string[] { DriveService.Scope.Drive,
                                 DriveService.Scope.DriveFile, DriveService.Scope.DriveMetadata};
                var userId = _userdata.Id;

                var credential = new UserCredential(Flow, userId, token);
                //var update = Flow.RefreshTokenAsync(userId, token.RefreshToken, CancellationToken.None).Result;
                if (credential.Token.IsExpired(credential.Flow.Clock))
                {
                    var refreshed = credential.RefreshTokenAsync(CancellationToken.None).Result;
                }

                Flow.DataStore.StoreAsync(userId, token);

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "PicAggo",
                });

                // if (_userdata.AppParentFolderId == null)
                // {
                var folderIds = GoogleDriveService.createDirectory(service, "PicAggo", "AppParent", "root");

                _userdata.AppParentFolderId = folderIds;
                UserManager.Update(_userdata);
                _db.SaveChanges();

                //}
                //else
                //{

                //}
                //check whether this user is already a part of any event and user group, then get event id and share event folder
                var uGroup = _db.UserGroupMappings.Any(x => x.UserName == _userdata.UserName);
                if (uGroup)
                {
                    var gId = _db.UserGroupMappings.Where(x => x.UserName == _userdata.UserName).Select(x => x.GroupId).ToList();
                    foreach (var item in gId)
                    {
                        if (_db.EventGroupMapping.Any(x => x.GroupId == item))
                        {
                            var eventId = _db.EventGroupMapping.Where(x => x.GroupId == item).Select(x => x.EventId).FirstOrDefault();
                            var folderId = _db.Events.Where(x => x.Id == eventId).Select(x => x.AlbumLocation).FirstOrDefault();
                            List<string> emails = new List<string>();
                            emails.Add(_userdata.Email);
                            if (folderId != null && emails != null && emails.Count != 0)
                            {
                                ShareFolder(folderId, emails, _userdata.Id);
                            }
                        }
                    }
                }
                var result = new ResponseModel
                {
                    Message = "Success",
                    Status = HttpStatusCode.OK,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
            catch (Exception ex)
            {
                var result = new ResponseModel
                {
                    Message = ex.ToString(),
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Route("CheckAccess")]
        [Authorize]
        public JObject CheckAccess()
        {
            string key = User.Identity.GetUserId();
            TokenResponse t = Flow.DataStore.GetAsync<TokenResponse>(key).Result;
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
             {
                 {"access_token", t.AccessToken}

             });
            GoogleAccessToken googletoken = new GoogleAccessToken();
            // TokenResponse token = new TokenResponse();
            string tokenEndpoint = "https://www.googleapis.com/oauth2/v3/tokeninfo";
            var credential = new UserCredential(Flow, key, t);
            //var update = Flow.RefreshTokenAsync(userId, token.RefreshToken, CancellationToken.None).Result;
            if (credential.Token.IsExpired(credential.Flow.Clock))
            {
                var refreshed = credential.RefreshTokenAsync(CancellationToken.None).Result;
                // Flow.DataStore.StoreAsync(key, refreshed);
            }
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var response = client.PostAsync(tokenEndpoint, content);
                var responseContent = response.Result.Content.ReadAsStringAsync();

                if (!response.Result.IsSuccessStatusCode)
                {
                    var result = new ResponseModel
                    {
                        Message = "Access Code Invalid or Revoked.",
                        Status = HttpStatusCode.BadRequest,
                        Data = null

                    };
                    var d = JsonConvert.SerializeObject(result);
                    var s = JObject.Parse(d);
                    return s;
                    //return "Access Code Invalid or Revoked.";
                }
                else
                {
                    var result = new ResponseModel
                    {
                        Message = responseContent.Result.ToString(),
                        Status = HttpStatusCode.OK,
                        Data = null

                    };
                    var d = JsonConvert.SerializeObject(result);
                    var s = JObject.Parse(d);
                    return s;
                }
            }
        }

        //Revoke Token
        [AcceptVerbs(HttpVerbs.Post)]
        [Route("RevokeAccess")]
        [Authorize]
        public JObject RevokeAccess()
        {
            string key = User.Identity.GetUserId();
            TokenResponse t = Flow.DataStore.GetAsync<TokenResponse>(key).Result;
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
             {
                 {"token", t.AccessToken}

             });
            GoogleAccessToken googletoken = new GoogleAccessToken();
            // TokenResponse token = new TokenResponse();
            string tokenEndpoint = "https://accounts.google.com/o/oauth2/revoke";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var response = client.PostAsync(tokenEndpoint, content);
                var responseContent = response.Result.Content.ReadAsStringAsync();

                if (!response.Result.IsSuccessStatusCode)
                {

                    var result = new ResponseModel
                    {
                        Message = "Error",
                        Status = HttpStatusCode.BadRequest,
                        Data = null

                    };
                    var d = JsonConvert.SerializeObject(result);
                    var s = JObject.Parse(d);
                    return s;
                }
                else
                {
                    var result = new ResponseModel
                    {
                        Message = "Success",
                        Status = HttpStatusCode.OK,
                        Data = null

                    };
                    var d = JsonConvert.SerializeObject(result);
                    var s = JObject.Parse(d);
                    return s;
                }
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Route("RemoveStorage")]
        [Authorize]
        public JObject RemoveStorage()
        {
            try
            {
                RevokeAccess();

                var _user = UserManager.Users.ToList();
                var _userdata = new ApplicationUser();
                var id = User.Identity.GetUserId();
                _userdata = _user.Where(x => x.Id == id).FirstOrDefault();

                //Remove FolderId    
                _userdata.AppParentFolderId = null;
                UserManager.Update(_userdata);
                _db.SaveChanges();

                var result = new ResponseModel
                {
                    Message = "Success",
                    Status = HttpStatusCode.OK,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;



            }
            catch (Exception ex)
            {
                var result = new ResponseModel
                {
                    Message = ex.ToString(),
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
        }


        [HttpPost]
        [Route("DeleteAccount")]
        [Authorize]
        public JObject DeleteAccount()
        {
            try
            {
                RemoveStorage();
                var result = new ResponseModel
                {
                    Message = "Account removed.",
                    Status = HttpStatusCode.OK,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
            catch (Exception ex)
            {
                var result = new ResponseModel
                {
                    Message = "Something went wrong!",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
        }


        [HttpPost]
        [Route("CreateService")]
        [Authorize]
        public DriveService CreateService(string accessToken, string refreshToken)
        {
            var access_token = accessToken;
            var refresh_token = refreshToken;

            var tokenResponse = new TokenResponse
            {
                AccessToken = access_token,
                RefreshToken = refreshToken,
            };

            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new EntityFrameworkDataStore("DefaultConnection")
            });

            var credential = new UserCredential(apiCodeFlow, User.Identity.GetUserId(), tokenResponse);

            var newService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PicAggo"
            });

            return newService;
        }

        public Google.Apis.Drive.v2.DriveService GetService_v2()
        {
            string id = User.Identity.GetUserId();
            // UserCredential credential;
            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { Scope.Drive },
                DataStore = new EntityFrameworkDataStore("DefaultConnection")
            });

            string _key = EntityFrameworkDataStore.GenerateStoredKey(id);
            string t = _tokendb.GoogleUserCredentials.Where(x => x.Key == _key).FirstOrDefault().Credentials;

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(t);
            var creds = new UserCredential(apiCodeFlow, id, result);

            var service = new Google.Apis.Drive.v2.DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "PicAggo",
            });
            return service;
        }


        [HttpPost]
        [Route("CreateEventFolder")]
        [Authorize]
        public string CreateEventFolder(string id, string parentId, int eventId)
        {
            try
            {
                var _events = _db.Events.Where(x => x.Id == eventId).FirstOrDefault();

                var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _clientID,
                        ClientSecret = _clientSecret
                    },
                    Scopes = new[] { DriveService.Scope.Drive },
                    DataStore = new EntityFrameworkDataStore("DefaultConnection")
                });

                string _key = EntityFrameworkDataStore.GenerateStoredKey(id);
                string t = _tokendb.GoogleUserCredentials.Where(x => x.Key == _key).FirstOrDefault().Credentials;

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(t);
                var creds = new UserCredential(apiCodeFlow, id, result);

                if (creds.Token.IsExpired(creds.Flow.Clock))
                {
                    var refreshed = creds.RefreshTokenAsync(CancellationToken.None).Result;
                }
                if (result != null)
                {
                    var service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = creds,
                        ApplicationName = "PicAggo",
                    });

                    var eventFolders = GoogleDriveService.createEventFolder(service, _events.EventName, _events.Description, parentId);

                    _events.AlbumLocation = eventFolders.Item1;
                    _events.ThumbLocation = eventFolders.Item2;
                    _db.Entry(_events).State = EntityState.Modified;
                    _db.SaveChanges();
                    return "Success";
                }
                else
                {
                    return "Google Token Error";
                }
              
            }
            catch (Exception ex)
            {

                return ex.Message.ToString();
            }
         
        }



        [HttpPost]
        [Route("ShareFolder")]
        [Authorize]
        public string ShareFolder(string folderId, List<string> users, string id)
        {
            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new EntityFrameworkDataStore("DefaultConnection")
            });

            string _key = EntityFrameworkDataStore.GenerateStoredKey(id);
            string t = _tokendb.GoogleUserCredentials.Where(x => x.Key == _key).FirstOrDefault().Credentials;

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(t);
            var creds = new UserCredential(apiCodeFlow, id, result);

            if (creds.Token.IsExpired(creds.Flow.Clock))
            {
                var refreshed = creds.RefreshTokenAsync(CancellationToken.None).Result;
            }
            if (result != null)
            {
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = creds,
                    ApplicationName = "PicAggo",
                });

                var share = GoogleDriveService.ShareEventFolder(service, folderId, users);
                return share.ToString();
            }
            else
            {
                return "Something went wrong";
            }
        }



        [HttpGet]
        public ActionResult GetGoogleDriveFiles(string id)
        {
            id = User.Identity.GetUserId();
            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new EntityFrameworkDataStore("DefaultConnection")
            });

            string _key = EntityFrameworkDataStore.GenerateStoredKey(id);
            string t = _tokendb.GoogleUserCredentials.Where(x => x.Key == _key).FirstOrDefault().Credentials;

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(t);
            var creds = new UserCredential(apiCodeFlow, id, result);

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "PicAggo",
            });

            return View(GoogleDriveService.GetDriveFiles(service));
        }

        [Route("GetFolderData")]
        [Authorize]
        [HttpGet]
        public string GetContainsInFolder(string folderId)
        {
            string id = User.Identity.GetUserId();
            //string appFolder = 
            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { Scope.Drive },
                DataStore = new EntityFrameworkDataStore("DefaultConnection")
            });

            var apiCodeFlowv2 = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { Google.Apis.Drive.v2.DriveService.Scope.Drive },
                DataStore = new EntityFrameworkDataStore("DefaultConnection")
            });

            string _key = EntityFrameworkDataStore.GenerateStoredKey(id);
            string t = _tokendb.GoogleUserCredentials.Where(x => x.Key == _key).FirstOrDefault().Credentials;

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(t);
            var creds = new UserCredential(apiCodeFlow, id, result);

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "PicAggo",
            });

            var servicev2 = new Google.Apis.Drive.v2.DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "PicAggo",
            });
            return (GoogleDriveService.GetContainsInFolder(folderId, service, servicev2)).ToString();
        }

        // GET: Home  
        // [Authorize]
        public ActionResult UploadFiles()
        {
            return View();
        }

        [HttpPost]
        public string GetToken(string userId)
        {
            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientID,
                    ClientSecret = _clientSecret
                },
                Scopes = new[] { DriveService.Scope.Drive },
                DataStore = new EntityFrameworkDataStore("DefaultConnection")
            });
            
            string _key = EntityFrameworkDataStore.GenerateStoredKey(userId);
            string t = _tokendb.GoogleUserCredentials.Where(x => x.Key == _key).FirstOrDefault().Credentials;
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(t);
            var creds = new UserCredential(apiCodeFlow, userId, result);

           // if (creds.Token.IsExpired(creds.Flow.Clock))
           // {
                var refreshed = creds.RefreshTokenAsync(CancellationToken.None).Result;
           // }
            return result.AccessToken;
        }

        //  [Authorize]
        [HttpPost]
        public JObject UploadFiles(HttpPostedFileBase[] files, int eventId)
        {
            //  int eventId = 40;
            try
            {
                string[] scopes = new string[] { DriveService.Scope.Drive,
                                 DriveService.Scope.DriveFile, DriveService.Scope.DriveMetadata};

                var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _clientID,
                        ClientSecret = _clientSecret
                    },
                    Scopes = new[] { DriveService.Scope.Drive },
                    DataStore = new EntityFrameworkDataStore("DefaultConnection")
                });
                var userId = User.Identity.GetUserId();
                string _key = EntityFrameworkDataStore.GenerateStoredKey(userId);
                string t = _tokendb.GoogleUserCredentials.Where(x => x.Key == _key).FirstOrDefault().Credentials;

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(t);
                var creds = new UserCredential(apiCodeFlow, userId, result);

                if (creds.Token.IsExpired(creds.Flow.Clock))
                {
                    var refreshed = creds.RefreshTokenAsync(CancellationToken.None).Result;
                }
                if (creds != null)
                {
                    var service = new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = creds,
                        ApplicationName = "PicAggo",
                    });
                    //  var parentId = GoogleDriveService.createDirectory(service, "PicAggo", "Test", "root");
                    var _userFolderId = _db.Events.Where(x => x.Id == eventId).FirstOrDefault().AlbumLocation;
                    var _thumbFolderId = _db.Events.Where(x => x.Id == eventId).FirstOrDefault().ThumbLocation;
                    if (files != null)
                    {
                        //Ensure model state is valid  
                        if (ModelState.IsValid)
                        {   //iterating through multiple file collection   
                            foreach (HttpPostedFileBase file in files)
                            {
                                //Checking file is available to save.  
                                if (file != null)
                                {

                                    GoogleDriveService.uploadFile(service, file, _userFolderId, _thumbFolderId);
                                    //assigning file uploaded status to ViewBag for showing message to user.  
                                    //ViewBag.UploadStatus = files.Count().ToString() + " files uploaded successfully.";
                                }

                            }
                        }
                    }


                }
                //else
                //{
                //    TempData["userName"] = result.RedirectUri.ToString();
                //    return new RedirectResult(result.RedirectUri);
                //}

                var res = new ResponseModel
                {
                    Message = "Success",
                    Status = HttpStatusCode.OK,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(res);
                var s = JObject.Parse(d);
                return s;
            }
            catch (Exception Ex)
            {

                Ex = new Exception(Ex.Message.ToString() + "-" + Ex.InnerException.ToString());
                var result = new ResponseModel
                {
                    Message = Ex.ToString(),
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
        }



        //Send Nexmo Verification Code
        //  [HttpPost]
        [AllowAnonymous]
        [Route("RequestOTP")]
        public JObject RequestOTP(string to)
        {
            //Uncomment on production/
            Session["phoneNumber"] = to;
            //var start = Client.NumberVerify.Verify(new NumberVerify.VerifyRequest
            //{
            //    number = to,
            //    brand = "PicAggo"
            //});

            //Session["requestId"] = start.request_id;
            //return start.status;
            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = null

            };
           // var d =JsonConvert.DeserializeObject<ResponseModel>(result);
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        //Verify Nexmo Verification Code
        //   [HttpPost]
        [AllowAnonymous]
        [Route("VerifyOTP")]
        public JObject VerifyOTP([System.Web.Http.FromBody]string code, [System.Web.Http.FromBody]string deviceToken, [System.Web.Http.FromBody] string phoneNumber, [System.Web.Http.FromBody]string requestId)
        {
            //Uncomment on production/
            //var result = Client.NumberVerify.Check(new NumberVerify.CheckRequest
            //{
            //    request_id = Session["requestId"].ToString(),
            //    code = code
            //});


            // if (result.status == "0")
            // {
            var phonuNumber = phoneNumber;

            var user = CheckUserAsync(phonuNumber, deviceToken);

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = user

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        [AllowAnonymous]
        [Route("ResendOTP")]
        public JObject ResendOTP(string to)
        {
            Session["phoneNumber"] = to;
            //var start = Client.NumberVerify.Verify(new NumberVerify.VerifyRequest
            //{
            //    number = to,
            //    brand = "PicAggo"
            //});

            //Session["requestId"] = start.request_id;


            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = null

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;

        }

        [Authorize]
        [Route("ChangeMobileNumber")]
        [HttpPost]
        public JObject ChangeMobileNumber(string oldPhoneNumber, string newPhoneNumber)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var ifExist = _db.Users.Any(x => x.UserName == newPhoneNumber || x.PhoneNumber == newPhoneNumber);
            if (!ifExist && user.PhoneNumber == oldPhoneNumber)
            {
                user.UserName = newPhoneNumber;
                user.PhoneNumber = newPhoneNumber;
                user.PhoneNumberConfirmed = false;
                UserManager.Update(user);

                RequestOTP(newPhoneNumber);
                var result = new ResponseModel
                {
                    Message = "Verification OTP sent on new number.",
                    Status = HttpStatusCode.OK,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
            else
            {
                var result1 = new ResponseModel
                {
                    Message = "Old Mobile Number does not exist.",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result1);
                var s = JObject.Parse(d);
                return s;
            }
        }

        public ApplicationUser CheckUserAsync(string phoneNumber, string deviceToken)
        {
            var user = UserManager.FindByName(phoneNumber);
            ApplicationDbContext db = new ApplicationDbContext();
            if (user == null)
            {
                RegisterBindingModel model = new RegisterBindingModel();
                var _invited = new List<InvitedUsers>();
                _invited = db.InvitedUsers.ToList();
                var invited = new InvitedUsers();
                invited = _invited.Where(x => x.PhoneNumber == phoneNumber).FirstOrDefault();
                if (invited != null)
                {
                    // if the user is InvitedUsers to an event, register the user, get the groupId of the event and add this user to the respective group in the UserGroup Mapping table                
                    model.PhoneNumber = phoneNumber;
                    model.DeviceToken = deviceToken;
                    var register = Register(model);

                    var _user = UserManager.FindByName(phoneNumber);
                    var _groupId = db.EventGroupMapping.Where(x => x.EventId == invited.EventId).Select(x => x.GroupId).SingleOrDefault();

                    UserGroupMapping userGroup = new UserGroupMapping();
                    userGroup.UserName = _user.UserName;
                    userGroup.GroupId = _groupId;
                    userGroup.IsInviteAccepted = true;
                    _db.UserGroupMappings.Add(userGroup);
                    _db.SaveChanges();


                    return _user;

                }
                else
                {
                    model.PhoneNumber = phoneNumber;
                    model.DeviceToken = deviceToken;
                    var register = Register(model);
                    return register;
                }

            }
            else
            {
                user.DeviceToken = deviceToken;
                user.PhoneNumber = phoneNumber;
                UserManager.Update(user);

                return user;
            }
        }

        public ApplicationUser Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ApplicationUser();
            }

            var user = new ApplicationUser() { UserName = model.PhoneNumber, Email = "", PhoneNumber = model.PhoneNumber, PhoneNumberConfirmed = true, DeviceToken = model.DeviceToken };

            IdentityResult result = UserManager.Create(user, "1234");

            if (!result.Succeeded)
            {
                // var error = GetErrorResult(result)
                return new ApplicationUser();
            }
            else
            {
                return user;
            }


        }

    }





    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        protected static readonly ILogger Logger = ApplicationContext.Logger.ForType<AuthCallbackController>();

        /// <summary>Gets the authorization code flow.</summary>
        protected IAuthorizationCodeFlow Flow { get { return FlowData.Flow; } }

        /// <summary>
        /// Gets the user identifier. Potential logic is to use session variables to retrieve that information.
        /// </summary>
        protected string UserId { get { return FlowData.GetUserId(this); } }

        /// <summary>
        /// The authorization callback which receives an authorization code which contains an error or a code.
        /// If a code is available the method exchange the coed with an access token and redirect back to the original
        /// page which initialized the auth process (using the state parameter).
        /// <para>
        /// The current timeout is set to 10 seconds. You can change the default behavior by setting 
        /// <see cref="System.Web.Mvc.AsyncTimeoutAttribute"/> with a different value on your controller.
        /// </para>
        /// </summary>
        /// <param name="authorizationCode">Authorization code response which contains the code or an error.</param>
        /// <param name="taskCancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>
        /// Redirect action to the state parameter or <see cref="OnTokenError"/> in case of an error.
        /// </returns>
        [AsyncTimeout(60000)]
        public async override Task<ActionResult> IndexAsync(AuthorizationCodeResponseUrl authorizationCode,
           CancellationToken taskCancellationToken)
        {
            if (string.IsNullOrEmpty(authorizationCode.Code))
            {
                var errorResponse = new TokenErrorResponse(authorizationCode);
                Logger.Info("Received an error. The response is: {0}", errorResponse);
                Debug.WriteLine("Received an error. The response is: {0}", errorResponse);
                return OnTokenError(errorResponse);
            }

            Logger.Debug("Received \"{0}\" code", authorizationCode.Code);
            Debug.WriteLine("Received \"{0}\" code", authorizationCode.Code);


            var returnUrl = Request.Url.ToString();
            returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?"));

            var token = await Flow.ExchangeCodeForTokenAsync(UserId, authorizationCode.Code, returnUrl,
                taskCancellationToken).ConfigureAwait(false);

            // Extract the right state.
            var oauthState = await AuthWebUtility.ExtracRedirectFromState(Flow.DataStore, UserId,
                authorizationCode.State).ConfigureAwait(false);

            return new RedirectResult(oauthState);
        }

        protected override Google.Apis.Auth.OAuth2.Mvc.FlowMetadata FlowData
        {
            get { return new AppFlowMetadata(); }
        }

        protected override ActionResult OnTokenError(TokenErrorResponse errorResponse)
        {
            throw new TokenResponseException(errorResponse);
        }


        //protected override Google.Apis.Auth.OAuth2.Mvc.FlowMetadata FlowData
        //{
        //    get { return new AppFlowMetadata(); }
        //}
    }
    public class GoogleAccessToken

    {

        public string access_token { get; set; }

        public string token_type { get; set; }


        public int expires_in { get; set; }

        public string id_token { get; set; }

        public string refresh_token { get; set; }

    }
}
