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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PicAggoAPI.Models;

namespace PicAggoAPI.Controllers
{
    public class GroupsMastersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        // GET: api/GroupsMasters
        public JObject GetGroupsMasters()
        {
            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = db.GroupsMasters

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
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

        public GroupsMastersController()
        {

        }

        public GroupsMastersController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }
        // GET: api/GroupsMasters/5
        //  [ResponseType(typeof(GroupMaster))]
        public JObject GetGroupsMaster(int id)
        {
            GroupMaster groupsMaster = db.GroupsMasters.Find(id);
            if (groupsMaster == null)
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
                Data = groupsMaster

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }


        // GET: api/GroupsMasters/5
       // [ResponseType(typeof(GroupMaster))]
        [Authorize]
        [Route("api/groupsmasters/getGroupsByUser")]
        public JObject GetGroupsByUser()
        {
            var id = User.Identity.GetUserId();
            string userId = db.Users.Where(x => x.Id == id).Select(x => x.Id).FirstOrDefault();
            List<GroupMaster> groupsMaster = db.GroupsMasters.Where(x => x.CreatedBy == userId).ToList();
            if (groupsMaster == null)
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
                Data = groupsMaster

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        // PUT: api/GroupsMasters/5
        //[ResponseType(typeof(void))]
        public JObject PutGroupsMaster(int id, GroupMaster groupsMaster)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
           // groupsMaster.CreatedAt = DateTime.Now;
           // groupsMaster.CreatedBy = (User.Identity.Name != null) ? User.Identity.Name : "admin";
            if (id != groupsMaster.Id)
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

            db.Entry(groupsMaster).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupsMasterExists(id))
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

        // POST: api/GroupsMasters
       // [ResponseType(typeof(GroupMaster))]
        public JObject PostGroupsMaster(GroupMaster groupsMaster)
        {
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            var id = User.Identity.GetUserId();
            groupsMaster.CreatedAt = DateTime.Now;
            groupsMaster.CreatedBy = (id != null) ? id : "admin";
            db.GroupsMasters.Add(groupsMaster);
            db.SaveChanges();

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = groupsMaster

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        // DELETE: api/GroupsMasters/5
        //[ResponseType(typeof(GroupMaster))]
        public JObject DeleteGroupsMaster(int id)
        {
            GroupMaster groupsMaster = db.GroupsMasters.Find(id);
            if (groupsMaster == null)
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

            db.GroupsMasters.Remove(groupsMaster);
            db.SaveChanges();

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = groupsMaster

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

        private bool GroupsMasterExists(int id)
        {
            return db.GroupsMasters.Count(e => e.Id == id) > 0;
        }
    }
}