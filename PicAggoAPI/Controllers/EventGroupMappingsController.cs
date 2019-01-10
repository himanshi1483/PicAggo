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
    public class EventGroupMappingsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/EventGroupMappings
        public JObject GetEventGroupMapping()
        {
            var result1 = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = db.EventGroupMapping

            };
            var d1 = JsonConvert.SerializeObject(result1);
            var s1 = JObject.Parse(d1);
            return s1;
        }

        // GET: api/EventGroupMappings/5
      //  [ResponseType(typeof(EventGroupMapping))]
        public JObject GetEventGroupMapping(int id)
        {
            EventGroupMapping eventGroupMapping = db.EventGroupMapping.Find(id);
            if (eventGroupMapping == null)
            {
                var result = new ResponseModel
                {
                    Message = "Not Found",
                    Status = HttpStatusCode.NotFound,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }

            var result1 = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = eventGroupMapping

            };
            var d1 = JsonConvert.SerializeObject(result1);
            var s1 = JObject.Parse(d1);
            return s1;
        }

        // PUT: api/EventGroupMappings/5
       // [ResponseType(typeof(void))]
        public JObject PutEventGroupMapping(int id, EventGroupMapping eventGroupMapping)
        {
            if (!ModelState.IsValid)
            {
                var result = new ResponseModel
                {
                    Message = "Invalid Request",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }

            if (id != eventGroupMapping.Id)
            {
                var result = new ResponseModel
                {
                    Message = "Invalid Request",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }

            db.Entry(eventGroupMapping).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventGroupMappingExists(id))
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

            var result2 = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = eventGroupMapping

            };
            var d2 = JsonConvert.SerializeObject(result2);
            var s2 = JObject.Parse(d2);
            return s2;
        }

        // POST: api/EventGroupMappings
      //  [ResponseType(typeof(EventGroupMapping))]
        public JObject PostEventGroupMapping(EventGroupMapping eventGroupMapping)
        {
            if (!ModelState.IsValid)
            {
                var result = new ResponseModel
                {
                    Message = "Invalid Request",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }

            db.EventGroupMapping.Add(eventGroupMapping);
            db.SaveChanges();

            var result1 = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = eventGroupMapping

            };
            var d1 = JsonConvert.SerializeObject(result1);
            var s1 = JObject.Parse(d1);
            return s1;
        }

        // DELETE: api/EventGroupMappings/5
        //[ResponseType(typeof(EventGroupMapping))]
        public JObject DeleteEventGroupMapping(int id)
        {
            EventGroupMapping eventGroupMapping = db.EventGroupMapping.Find(id);
            if (eventGroupMapping == null)
            {
                var result = new ResponseModel
                {
                    Message = "Not Found",
                    Status = HttpStatusCode.NotFound,
                    Data = null

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }

            db.EventGroupMapping.Remove(eventGroupMapping);
            db.SaveChanges();

            var result1 = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = eventGroupMapping

            };
            var d1 = JsonConvert.SerializeObject(result1);
            var s1 = JObject.Parse(d1);
            return s1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EventGroupMappingExists(int id)
        {
            return db.EventGroupMapping.Count(e => e.Id == id) > 0;
        }
    }
}