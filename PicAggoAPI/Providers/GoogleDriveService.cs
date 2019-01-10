using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using PicAggoAPI.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Web;
using DriveService = Google.Apis.Drive.v3.DriveService;
using File = Google.Apis.Drive.v3.Data.File;
using FilesResource = Google.Apis.Drive.v3.FilesResource;

namespace PicAggoAPI.Providers
{
    public class GoogleDriveService
    {
        private readonly DriveService _service;
        private static readonly IDataStore dataStore;

        //// 

        /// Create a new Directory.
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// 
        /// a Valid authenticated DriveService
        /// The title of the file. Used to identify file or folder name.
        /// A short description of the file.
        /// Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.
        /// 
        public static string createDirectory(DriveService _service, string _title, string _description, string _parent)
        {

            Google.Apis.Drive.v3.Data.File NewDirectory = null;

            // Create metaData for a new Directory
            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
            body.Name = _title + "_"+ DateTime.Today.Ticks;
            body.Description = _description;
            body.MimeType = "application/vnd.google-apps.folder";
            //body.Parents = "root";
            if (!string.IsNullOrEmpty(_parent))
            {
                body.Parents = new List<string>() { "root" };
            };
            //create parent folder for app
            try
            {
                //Google.Apis.Drive.v3.FilesResource.ListRequest checkDirectory = _service.Files.List();
               
                //checkDirectory.Q = "name='PicAggo'";
               
               
               // string folderId = checkDirectory.Execute().Files[0].Id;

                FilesResource.CreateRequest request = _service.Files.Create(body);
                NewDirectory = request.Execute();
                return NewDirectory.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return "";
            }

        }


        public static Tuple<string, string> createEventFolder(DriveService _service, string _title, string _description, string _parent)
        {

            Google.Apis.Drive.v3.Data.File NewDirectory = null;

            // Create metaData for a new Directory
            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
            body.Name = _title;
            body.Description = _description;
            body.MimeType = "application/vnd.google-apps.folder";
            //body.Parents = "root";
            if (!string.IsNullOrEmpty(_parent))
            {
                body.Parents = new List<string>() { _parent };
            };

            //create parent folder for app
            try
            {
                FilesResource.ListRequest checkDirectory = _service.Files.List();

                FilesResource.CreateRequest request = _service.Files.Create(body);

                NewDirectory = request.Execute();

            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            Console.WriteLine(NewDirectory.Id);

            //create thumbnail folder inside parent folder

            Google.Apis.Drive.v3.Data.File thumbDirectory = null;
            Google.Apis.Drive.v3.Data.File thumbbody = new Google.Apis.Drive.v3.Data.File();
            thumbbody.Name = "Thumbs";
            thumbbody.Description = _description;
            thumbbody.MimeType = "application/vnd.google-apps.folder";
            //thumbbody.Parents = NewDirectory.Id;
            if (!string.IsNullOrEmpty(NewDirectory.Id))
            {
                thumbbody.Parents = new List<string>() { NewDirectory.Id };
            };
            try
            {
                FilesResource.ListRequest checkDirectory = _service.Files.List();

                FilesResource.CreateRequest request = _service.Files.Create(thumbbody);
                thumbDirectory = request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return Tuple.Create(NewDirectory.Id, thumbDirectory.Id);
        }

        public static string ShareEventFolder(DriveService _service, string folderId, List<string> contributers)
        {
            try
            {
                Google.Apis.Drive.v3.Data.Permission permission = new Google.Apis.Drive.v3.Data.Permission();
                foreach (var item in contributers)
                {
                    permission.Type = "user";
                    permission.EmailAddress = item;
                    permission.Role = "writer";
                    permission = _service.Permissions.Create(permission, folderId).Execute();
                }
                return "success";
            }
            catch (Exception ex)
            {

                return ex.ToString();
            }
            
        }
        // tries to figure out the mime type of the file.
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
            {
                mimeType = regKey.GetValue("Content Type").ToString();
            }

            return mimeType;
        }

        /// 

        /// Uploads a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/insert
        /// 
        /// a Valid authenticated DriveService
        /// path to the file to upload
        /// Collection of parent folders which contain this file. 
        ///                       Setting this field will put the file in all of the provided folders. root folder.
        /// If upload succeeded returns the File resource of the uploaded file 
        ///          If the upload fails returns null
        public static Google.Apis.Drive.v3.Data.File uploadFile(DriveService _service, HttpPostedFileBase _uploadFile, string _parent, string thumbparent)
        {
            //if (System.IO.File.Exists(_uploadFile))
            // {
            File body = new File();
            body.Name = System.IO.Path.GetFileName(_uploadFile.FileName);
            body.Description = "File uploaded by Drive Sample";
            body.MimeType = GetMimeType(_uploadFile.FileName);
            if (!string.IsNullOrEmpty(_parent))
            {
                body.Parents = new List<string>() { _parent };
            }


            var fileMetadata = new File()
            {
                Name = _uploadFile.FileName,
                Parents = new List<string>
                {
                    _parent
                }
            };

            // File's content.
            System.IO.BinaryReader b = new System.IO.BinaryReader(_uploadFile.InputStream);
            byte[] byteArray = b.ReadBytes(_uploadFile.ContentLength);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, GetMimeType(_uploadFile.FileName));
                request.Upload();

                //create thumbnail
                Image image = Image.FromStream(stream);
                // Compute thumbnail size.
                Size thumbnailSize = ThumbnailHelper.GetThumbnailSize(image);

                //Image thumbnail = image.GetThumbnailImage(thumbnailSize.Width,
                //    thumbnailSize.Height, null, IntPtr.Zero);

                File thumbbody = new File();
                thumbbody.Name = System.IO.Path.GetFileName(_uploadFile.FileName);
                thumbbody.Description = "File uploaded by Drive Sample";
                thumbbody.MimeType = GetMimeType(_uploadFile.FileName);

                if (!string.IsNullOrEmpty(thumbparent))
                {
                    thumbbody.Parents = new List<string>() { thumbparent };
                }
                // Get thumbnail.
                using (Image thumbPhoto = image.GetThumbnailImage(thumbnailSize.Width, thumbnailSize.Height, null, IntPtr.Zero))
                {
                    // The below code converts an Image object to a byte array
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        thumbPhoto.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] imgBytes = ms.ToArray();
                        FilesResource.CreateMediaUpload requestthumb = _service.Files.Create(thumbbody, ms, GetMimeType(_uploadFile.FileName));
                        requestthumb.Upload();
                    }
                }

                return request.ResponseBody;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
            //}
            //else
            //{
            //    Console.WriteLine("File does not exist: " + _uploadFile);
            //    return null;
            //}

        }

        private static File insertFile(DriveService service, String title, String description, String parentId, String mimeType, HttpPostedFileBase filename)
        {
            // File's metadata.
            File body = new File();
            body.Name = title;
            body.Description = description;
            body.MimeType = mimeType;

            // Set the parent folder.
            if (!String.IsNullOrEmpty(parentId))
            {
                body.Parents = new List<String>() { parentId };
            }

            // File's content.
            System.IO.BinaryReader b = new System.IO.BinaryReader(filename.InputStream);
            byte[] byteArray = b.ReadBytes(filename.ContentLength);
            //  byte[] byteArray = System.IO.File.ReadAllBytes(filename);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

            try
            {
                FilesResource.CreateMediaUpload request = service.Files.Create(body, stream, mimeType);
                request.Upload();

                File file = request.ResponseBody;

                // Uncomment the following line to print the File ID.
                // Console.WriteLine("File ID: " + file.Id);

                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }


        /// <summary>
        /// ** Installed Aplication only ** 
        /// This method requests Authentcation from a user using Oauth2.  
        /// </summary>
        /// <param name="clientSecretJson">Path to the client secret json file from Google Developers console.</param>
        /// <param name="userName">Identifying string for the user who is being authentcated.</param>
        /// <param name="scopes">Array of Google scopes</param>
        /// <returns>DriveService used to make requests against the Drive API</returns>
        public static DriveService GetDriveService(string clientSecretJson, string userName, string[] scopes)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    throw new ArgumentNullException("userName");
                }

                if (string.IsNullOrEmpty(clientSecretJson))
                {
                    throw new ArgumentNullException("clientSecretJson");
                }

                if (!System.IO.File.Exists(clientSecretJson))
                {
                    throw new Exception("clientSecretJson file does not exist.");
                }

                var cred = GetUserCredential(clientSecretJson, userName, scopes);
                return GetService(cred);

            }
            catch (Exception ex)
            {
                throw new Exception("Get Drive service failed.", ex);
            }
        }

        /// <summary>
        /// ** Installed Aplication only ** 
        /// This method requests Authentcation from a user using Oauth2.  
        /// Credentials are stored in System.Environment.SpecialFolder.Personal
        /// Documentation https://developers.google.com/accounts/docs/OAuth2
        /// </summary>
        /// <param name="clientSecretJson">Path to the client secret json file from Google Developers console.</param>
        /// <param name="userName">Identifying string for the user who is being authentcated.</param>
        /// <param name="scopes">Array of Google scopes</param>
        /// <returns>authencated UserCredential</returns>
        private static UserCredential GetUserCredential(string clientSecretJson, string userName, string[] scopes)
        {
            try
            {
                if (string.IsNullOrEmpty(userName))
                {
                    throw new ArgumentNullException("userName");
                }

                if (string.IsNullOrEmpty(clientSecretJson))
                {
                    throw new ArgumentNullException("clientSecretJson");
                }

                if (!System.IO.File.Exists(clientSecretJson))
                {
                    throw new Exception("clientSecretJson file does not exist.");
                }

                // These are the scopes of permissions you need. It is best to request only what you need and not all of them               
                using (var stream = new System.IO.FileStream(clientSecretJson, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    //string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    // credPath = System.IO.Path.Combine(credPath, ".credentials/", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

                    // Requesting Authentication or loading previously stored authentication for userName
                    //var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                    //                                                         scopes,
                    //                                                         userName,
                    //                                                         CancellationToken.None,
                    //                                                         new FileDataStore(credPath, true)).Result;

                    var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                                                                          scopes,
                                                                          userName,
                                                                          CancellationToken.None,
                                                                          dataStore).Result;

                    credential.GetAccessTokenForRequestAsync();
                    return credential;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Get user credentials failed.", ex);
            }
        }

        /// <summary>
        /// This method get a valid service
        /// </summary>
        /// <param name="credential">Authecated user credentail</param>
        /// <returns>DriveService used to make requests against the Drive API</returns>
        private static DriveService GetService(UserCredential credential)
        {
            try
            {
                if (credential == null)
                {
                    throw new ArgumentNullException("credential");
                }

                // Create Drive API service.
                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "PicAggo"
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Get Drive service failed.", ex);
            }
        }


        public static List<GoogleDriveFiles> GetContainsInFolder(String folderId, DriveService _service, Google.Apis.Drive.v2.DriveService servicev2)
        {
            GoogleDriveFiles AppFolder = GetDriveFiles(_service).Where(x => x.Name == "PicAggo").FirstOrDefault();
            string appFolderId = AppFolder.Id;
            List<string> ChildList = new List<string>();
           
            Google.Apis.Drive.v2.DriveService ServiceV2 = servicev2;
            ChildrenResource.ListRequest IDsRequest = servicev2.Children.List(appFolderId);
           
            do
            {
               // var children = new List<string>();
                ChildList children = IDsRequest.Execute();

                if (children.Items != null && children.Items.Count > 0)
                {
                    ChildrenResource.ListRequest ChildrenIDsRequest = servicev2.Children.List(folderId);
                    foreach (var file in children.Items)
                    {
                        ChildList.Add(file.Id);
                    }
                }
                IDsRequest.PageToken = children.NextPageToken;

            } while (!String.IsNullOrEmpty(IDsRequest.PageToken));

            //Get All File List
            List<GoogleDriveFiles> AllFileList = GetDriveFiles(_service).Where(x => x.Name == "PicAggo").ToList();
            List<GoogleDriveFiles> Filter_FileList = new List<GoogleDriveFiles>();

            foreach (string Id in ChildList)
            {
                Filter_FileList.Add(AllFileList.Where(x => x.Id == Id).FirstOrDefault());
            }
            return Filter_FileList;
        }

       

        public static List<GoogleDriveFiles> GetDriveFiles(DriveService _service)
        {
            //Google.Apis.Drive.v3.DriveService service = GetService_v3();

            // Define parameters of request.
            Google.Apis.Drive.v3.FilesResource.ListRequest FileListRequest = _service.Files.List();
            FileListRequest.Fields = "nextPageToken, files(createdTime, id, name, size, version, trashed, parents)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<GoogleDriveFiles> FileList = new List<GoogleDriveFiles>();

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    GoogleDriveFiles File = new GoogleDriveFiles
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime,
                        Parents = file.Parents
                    };
                    FileList.Add(File);
                }
            }
            return FileList;
        }

    }

    internal class ParentReference
    {
        public ParentReference()
        {
        }

        public string Id { get; set; }
    }
}
