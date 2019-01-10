using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PicAggoAPI.Models;

namespace PicAggoAPI.Controllers
{
    public class UserGroupMappingsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/UserGroupMappings
        public JObject GetUserGroupMappings()
        {

            var result1 = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = db.UserGroupMappings

            };
            var d1 = JsonConvert.SerializeObject(result1);
            var s1 = JObject.Parse(d1);
            return s1;
        }

        // GET: api/UserGroupMappings/5
        //[ResponseType(typeof(UserGroupMapping))]
        public JObject GetUserGroupMapping(int id)
        {
            UserGroupMapping userGroupMapping = db.UserGroupMappings.Find(id);
            if (userGroupMapping == null)
            {
                var result1 = new ResponseModel
                {
                    Message = "Not found",
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
                Data = userGroupMapping

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        // PUT: api/UserGroupMappings/5
       // [ResponseType(typeof(void))]
        public JObject PutUserGroupMapping(int id, UserGroupMapping userGroupMapping)
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

            if (id != userGroupMapping.Id)
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

            db.Entry(userGroupMapping).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserGroupMappingExists(id))
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

        // POST: api/UserGroupMappings
       // [ResponseType(typeof(List<UserGroupMapping>))]
        public JObject PostUserGroupMapping(List<UserGroupMapping> userGroupList)
        {
            if (!ModelState.IsValid)
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

            foreach (var item in userGroupList)
            {
                db.UserGroupMappings.Add(item);
            }
          //  db.UserGroupMappings.Add(userGroupMapping);
            db.SaveChanges();

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = userGroupList

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        // DELETE: api/UserGroupMappings/5
       // [ResponseType(typeof(UserGroupMapping))]
        public JObject DeleteUserGroupMapping(int id)
        {
            UserGroupMapping userGroupMapping =  db.UserGroupMappings.Find(id);
            if (userGroupMapping == null)
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

            db.UserGroupMappings.Remove(userGroupMapping);
            db.SaveChanges();

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = userGroupMapping

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

        private bool UserGroupMappingExists(int id)
        {
            return db.UserGroupMappings.Count(e => e.Id == id) > 0;
        }
    }
}