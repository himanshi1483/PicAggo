using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PicAggoAPI.Models;
using PicAggoAPI.Providers;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace PicAggoAPI.Controllers
{
    public class EventDetailsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        FireBasePush push = new FireBasePush("AIzaSyBBJlyHtzcdJ1eOQgjgb9OLfSwGaY3LRlM");
        // GET: api/EventDetails
        public JObject GetEventDetails()
        {
            var result1 = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = db.EventDetails

            };
            var d1 = JsonConvert.SerializeObject(result1);
            var s1 = JObject.Parse(d1);
            return s1;
        }


        // GET: api/EventDetails/5
        //  [ResponseType(typeof(EventDetails))]
        [Authorize]
        public JObject GetEventDetails(int id)
        {
            List<EventDetails> eventDetails = db.EventDetails.Where(x => x.EventId == id).ToList();
            if (eventDetails == null)
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

            var userId = User.Identity.GetUserId();
            HomeController home = new HomeController();
            var accessToken = home.GetToken(userId);

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = eventDetails,
                AccessToken = accessToken

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
        }

        // PUT: api/EventDetails/5
        //[ResponseType(typeof(void))]
        public JObject PutEventDetails(int id, EventDetails eventDetails)
        {
            if (!ModelState.IsValid)
            {
                var result1 = new ResponseModel
                {
                    Message = "Invalid request",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d1 = JsonConvert.SerializeObject(result1);
                var s1 = JObject.Parse(d1);
                return s1;
                // return BadRequest(ModelState);
            }

            if (id != eventDetails.Id)
            {
                var result2 = new ResponseModel
                {
                    Message = "Invalid request",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d2 = JsonConvert.SerializeObject(result2);
                var s2 = JObject.Parse(d2);
                return s2;
            }

            db.Entry(eventDetails).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
                //list of collaborators
                var groupId = db.EventGroupMapping.Where(x => x.EventId == eventDetails.EventId).First().GroupId;
                var list = db.UserGroupMappings.Where(x => x.GroupId == groupId).ToList();
                List<string> userTokens = new List<string>();
                foreach (var item in list)
                {
                    var token = db.Users.Where(x => x.UserName == item.UserName).First().DeviceToken;
                    userTokens.Add(token);
                }
                string joined = string.Join(",", userTokens);

                push.SendPush(new PushMessage()
                {
                    to = joined, //for a topic to": "/topics/foo-bar"
                    notification = new PushMessageData
                    {
                        title = "Event Update",
                        text = "Event status has changed",
                        click_action = ""
                    },
                    data = new
                    {
                        example = "Test"
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventDetailsExists(id))
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

        // POST: api/EventDetails
        //  [ResponseType(typeof(EventDetails))]
        public JObject PostEventDetails(EventDetails eventDetails)
        {
            if (eventDetails.EventId == null || eventDetails.EventId == 0)
            {
                var result1 = new ResponseModel
                {
                    Message = "Invalid parameters.",
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d1 = JsonConvert.SerializeObject(result1);
                var s1 = JObject.Parse(d1);
                return s1;

            }

            eventDetails.ActivityBy = User.Identity.GetUserId();

            db.EventDetails.Add(eventDetails);


            if (eventDetails.SessionActivity == "SessionEnded")
            {
                var _event = db.Events.Find(eventDetails.EventId);
                _event.EndTime = eventDetails.ActivityTime;
                db.Entry(_event).State = EntityState.Modified;

            }
            //if (eventDetails.SessionActivity == "SessionStarted")
            //{
            //    var _event = db.Events.Find(eventDetails.EventId);
            //    _event.StartTIme = eventDetails.ActivityTime;
            //    db.Entry(_event).State = EntityState.Modified;

            //}
            db.SaveChanges();

            var groupId = db.EventGroupMapping.Where(x => x.EventId == eventDetails.EventId).First().GroupId;
            var list = db.UserGroupMappings.Where(x => x.GroupId == groupId).ToList();
            List<string> userTokens = new List<string>();
            foreach (var item in list)
            {
                var token = db.Users.Where(x => x.UserName == item.UserName).First().DeviceToken;
                userTokens.Add(token);
            }
            string joined = string.Join(",", userTokens);

            push.SendPush(new PushMessage()
            {
                to = joined, //for a topic to": "/topics/foo-bar"
                notification = new PushMessageData
                {
                    title = "Event Update",
                    text = "Event status has changed",
                    click_action = ""
                },
                data = new
                {
                    example = "Test"
                }
            });

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = eventDetails

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
            //return CreatedAtRoute("DefaultApi", new { id = eventDetails.Id }, eventDetails);
        }

        // DELETE: api/EventDetails/5
        //[ResponseType(typeof(EventDetails))]
        public JObject DeleteEventDetails(int id)
        {
            EventDetails eventDetails = db.EventDetails.Find(id);
            if (eventDetails == null)
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

            db.EventDetails.Remove(eventDetails);
            db.SaveChanges();

            var result1 = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = eventDetails

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

        private bool EventDetailsExists(int id)
        {
            return db.EventDetails.Count(e => e.Id == id) > 0;
        }
    }
}