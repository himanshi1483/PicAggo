using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace PicAggoAPI.Controllers
{
    public class GoogleDriveController : ApiController
    {
        private readonly DriveService _service;
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
        public static Google.Apis.Drive.v3.Data.File createDirectory(DriveService _service, string _title, string _description, string _parent)
        {

            Google.Apis.Drive.v3.Data.File NewDirectory = null;

            // Create metaData for a new Directory
            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
            body.Name = _title;
            body.Description = _description;
            body.MimeType = "application/vnd.google-apps.folder";
          //  body.Parents = new IList() { new ParentReference() { Id = _parent } };
            try
            {
                FilesResource.CreateRequest request = _service.Files.Create(body);
                NewDirectory = request.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return NewDirectory;
        }

        // tries to figure out the mime type of the file.
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
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
        public static Google.Apis.Drive.v3.Data.File uploadFile(DriveService _service, string _uploadFile, string _parent)
        {

            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File();
                body.Name = System.IO.Path.GetFileName(_uploadFile);
                body.Description = "File uploaded by Diamto Drive Sample";
                body.MimeType = GetMimeType(_uploadFile);
                //body.Parents = new IList() { new ParentReference() { Id = _parent } };

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
                try
                {
                    FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + _uploadFile);
                return null;
            }

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
