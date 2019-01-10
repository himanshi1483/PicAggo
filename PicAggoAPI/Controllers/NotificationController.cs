using PicAggoAPI.Models;
using PicAggoAPI.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace PicAggoAPI.Controllers
{
    public class NotificationController : ApiController
    {

        public void SendNotification(Byte[] byteArray)
        {
            try
            {
                string SERVER_API_KEY = "AAAAcLCDcp4:" + "APA91bGENpz5khBUpBXdZpnwZuh68fKHd7Nq_0zc9LSzYaAfxb4B6bIF24r2_Vd6jPb4xWrkiMTjWqT08oLuHKPsj06zfgYx7ZFVq76qkzJJevCHzs4NNIvA6MLkFWq9i3BKTcVikFic";
                var SENDER_ID = "483997741726";
                // Create Request
                WebRequest tRequest;
                tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");     // FCM link
                tRequest.Method = "post";
                tRequest.ContentType = "application / json";
                tRequest.Headers.Add(string.Format("Authorization: key ={0}", SERVER_API_KEY));     //Server Api Key Header
                tRequest.Headers.Add(string.Format("Sender: id ={0}", SENDER_ID));     // Sender Id Header
                tRequest.ContentLength = byteArray.Length;
                Stream dataStream = tRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse tResponse = tRequest.GetResponse();
                dataStream = tResponse.GetResponseStream();
                StreamReader tReader = new StreamReader(dataStream);
                String sResponseFromServer = tReader.ReadToEnd();
                tReader.Close();
                dataStream.Close();
                tResponse.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
