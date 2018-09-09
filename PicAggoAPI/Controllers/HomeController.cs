using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Nexmo.Api;
using PicAggoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PicAggoAPI.Controllers
{
    // [Route("api/Verification")]
    public class HomeController : Controller
    {
        public Client Client { get; set; }
        private ApplicationUserManager _userManager;
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

        public ActionResult Index()
        {
            return View();
        }


        //Send Nexmo Verification Code
        //  [HttpPost]
        [AllowAnonymous]
        [Route("RequestOTP")]
        public string RequestOTP(string to)
        {
            Session["phoneNumber"] = to;
            var start = Client.NumberVerify.Verify(new NumberVerify.VerifyRequest
            {
                number = to,
                brand = "PicAggo"
            });

            Session["requestId"] = start.request_id;

            //if (start.status == "0")
            //{
            //    return "OTP Sent";
            //}
            //else
            //{
                return start.status;
           // }
        }

        //Verify Nexmo Verification Code
        //   [HttpPost]
        [AllowAnonymous]
        [Route("VerifyOTP")]
        public string VerifyOTP(string code, string deviceToken)
        {

            var result = Client.NumberVerify.Check(new NumberVerify.CheckRequest
            {
                request_id = Session["requestId"].ToString(),
                code = code
            });


            if (result.status == "0")
            {
                var phonuNumber = Session["phoneNumber"].ToString();
                
                var user = CheckUserAsync(phonuNumber, deviceToken);
              
                return result.status.ToString();
            }
            else
            {
                return result.error_text;
            }
        }

        public string CheckUserAsync(string phoneNumber, string deviceToken)
        {
            var user =  UserManager.FindByName(phoneNumber);
            if (user == null)
            {
                RegisterBindingModel model = new RegisterBindingModel();
                model.PhoneNumber = phoneNumber;
                model.DeviceToken = deviceToken;
                var register = Register(model);
                return register.ToString();
            }
            else
            {
                user.DeviceToken = deviceToken;
                UserManager.Update(user);
                return "Already Exist";
            }
        }

        public bool Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return false;
            }

            var user = new ApplicationUser() { UserName = model.PhoneNumber, Email = "", PhoneNumber = model.PhoneNumber, PhoneNumberConfirmed = true, DeviceToken = model.DeviceToken };

            IdentityResult result = UserManager.Create(user, "1234");

            if (!result.Succeeded)
            {
               // var error = GetErrorResult(result)
                return false;
            }
            else
            {
                return true;
            }


        }

    }
}
