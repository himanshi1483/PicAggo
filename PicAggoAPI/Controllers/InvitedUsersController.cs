using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PicAggoAPI.Models;

namespace PicAggoAPI.Controllers
{
    public class InvitedUsersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/InvitedUsers
        public JObject GetInvitedUsers()
        {
            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = db.InvitedUsers

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        // GET: api/InvitedUsers/5
       // [ResponseType(typeof(InvitedUsers))]
        public JObject GetInvitedUsers(int id)
        {
            InvitedUsers invitedUsers = db.InvitedUsers.Find(id);
            if (invitedUsers == null)
            {
                var result1 = new ResponseModel
                {
                    Message = "Not Found",
                    Status = HttpStatusCode.NotFound,
                    Data = null

                };
                var d1 = JsonConvert.SerializeObject(result1);
                var s1 = JObject.Parse(d1);
                return s1;
            }

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = invitedUsers

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        // PUT: api/InvitedUsers/5
        //[ResponseType(typeof(void))]
        public JObject PutInvitedUsers(int id, InvitedUsers invitedUsers)
        {
            if (!ModelState.IsValid)
            {
                var result1 = new ResponseModel
                {
                    Message = "Invalid Request",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d1 = JsonConvert.SerializeObject(result1);
                var s1 = JObject.Parse(d1);
                return s1;
            }

            if (id != invitedUsers.Id)
            {
                var result1 = new ResponseModel
                {
                    Message = "Invalid Request",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d1 = JsonConvert.SerializeObject(result1);
                var s1 = JObject.Parse(d1);
                return s1;
            }

            db.Entry(invitedUsers).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvitedUsersExists(id))
                {
                    var result1 = new ResponseModel
                    {
                        Message = "Not Found",
                        Status = HttpStatusCode.NotFound,
                        Data = null

                    };
                    var d1 = JsonConvert.SerializeObject(result1);
                    var s1 = JObject.Parse(d1);
                    return s1;
                }
                else
                {
                    throw;
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

        // POST: api/InvitedUsers
        //[ResponseType(typeof(InvitedUsers))]
        public JObject PostInvitedUsers(InvitedUsers invitedUsers)
        {
            if (!ModelState.IsValid)
            {
                var result1 = new ResponseModel
                {
                    Message = "Invalid Request",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d1 = JsonConvert.SerializeObject(result1);
                var s1 = JObject.Parse(d1);
                return s1;
            }

            db.InvitedUsers.Add(invitedUsers);
            db.SaveChanges();

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = invitedUsers

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        // DELETE: api/InvitedUsers/5
       // [ResponseType(typeof(InvitedUsers))]
        public JObject DeleteInvitedUsers(int id)
        {
            InvitedUsers invitedUsers = db.InvitedUsers.Find(id);
            if (invitedUsers == null)
            {
                var result1 = new ResponseModel
                {
                    Message = "Not Found",
                    Status = HttpStatusCode.NotFound,
                    Data = null

                };
                var d1 = JsonConvert.SerializeObject(result1);
                var s1 = JObject.Parse(d1);
                return s1;
            }

            db.InvitedUsers.Remove(invitedUsers);
            db.SaveChanges();

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = invitedUsers

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool InvitedUsersExists(int id)
        {
            return db.InvitedUsers.Count(e => e.Id == id) > 0;
        }
    }
}