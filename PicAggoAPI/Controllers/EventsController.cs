using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PicAggoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;

namespace PicAggoAPI.Controllers
{
    public class EventsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public EventsController()
        {

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
        public EventsController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        // GET: api/Events
        [Authorize]
        public JObject GetEvents()
        {
            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = db.Events

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
            //  return db.Events;
        }

        // GET: api/Events
        [Authorize]
        [Route("api/Events/getEventsByUser")]
        public JObject GetEventsByUser()
        {
            string Id = User.Identity.GetUserId();
            var events = db.Events.Where(x => x.CreatedBy == Id).ToList();
            foreach (var item in events)
            {
                var invitedUsers = db.InvitedUsers.Where(x => x.EventId == item.Id).Select(x => x.PhoneNumber).ToList();
                item.InvitedUsers = invitedUsers;
                var gId = db.EventGroupMapping.Where(x => x.EventId == item.Id).Select(x => x.GroupId).SingleOrDefault();
                item.GroupId = gId;
            }

            if (events.Count == 0)
            {
                var result = new ResponseModel
                {
                    Message = "No event found",
                    Status = HttpStatusCode.OK,
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
                    Data = events

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }


            //   return db.Events.Where(x => x.CreatedBy == userName);
        }

        // GET: api/Events
        [Route("api/Events/getCurrentEventsByUser")]
        [Authorize]
        public JObject GetCurrentEventsByUser()
        {
            string Id = User.Identity.GetUserId();
            var events = db.Events.Where(x => x.CreatedBy == Id && x.EndTime == null).ToList();
            if (events.Count == 0)
            {
                var result = new ResponseModel
                {
                    Message = "No event found",
                    Status = HttpStatusCode.OK,
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
                    Data = db.Events.Where(x => x.CreatedBy == Id && x.EndTime == null)

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
        }



        // GET: api/Events
        [Route("api/Events/getCurrentEvents")]
        [Authorize]
        public IQueryable<Events> GetCurrentEvents()
        {
            return db.Events.Where(x => x.EndTime == null);
        }



        // GET: api/Events
        [Route("api/Events/getEventStatus")]
        [Authorize]
        public JObject GetEventStatus(int eventId)
        {
            var _event = db.Events.Where(x => x.Id == eventId).FirstOrDefault();
            if (_event.EndTime != null)
            {
                _event.RemainingDuration = "0";
                var result = new ResponseModel
                {
                    Message = "Success",
                    Status = HttpStatusCode.OK,
                    Data = "Session Ended."

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
            else
            {
                var lastActivity = "";
                var eventDuration = _event.Duration;
                var _eventDetail = db.EventDetails.Where(x => x.EventId == eventId).ToList().OrderByDescending(x => x.ActivityTime);
                if(_eventDetail.Count() == 0)
                {
                    lastActivity = "Session Not Started";
                }
                else
                {
                    var startTime = _eventDetail.Where(x => x.SessionActivity == "SessionStarted").Select(x => x.ActivityTime).FirstOrDefault();
                    var _remainingTime = DateTime.Now.TimeOfDay.Subtract(startTime.TimeOfDay).Minutes;
                    lastActivity = _eventDetail.Select(x => x.SessionActivity).FirstOrDefault();
                    _event.RemainingDuration = (Convert.ToInt32(_event.Duration) - Convert.ToInt32(_remainingTime)).ToString();
                }
              
                // return lastActivity.ToString() + " at " + _eventDetail.First().ActivityTime; 
                var result = new ResponseModel
                {
                    Message = "Success",
                    Status = HttpStatusCode.OK,
                    Data = lastActivity

                };
                var d = JsonConvert.SerializeObject(result);
                var s = JObject.Parse(d);
                return s;
            }
        }



        // GET: api/Events/5
        //[ResponseType(typeof(Events))]
        [Authorize]
        public JObject GetEvents(int id)
        {
            Events events = db.Events.Find(id);
            var invitedUsers = db.InvitedUsers.Where(x => x.EventId == id).Select(x => x.PhoneNumber).ToList();
            events.InvitedUsers = invitedUsers;
            var gId = db.EventGroupMapping.Where(x => x.EventId == id).Select(x => x.GroupId).SingleOrDefault();
            events.GroupId = gId;

            if (events == null)
            {
                var result = new ResponseModel
                {
                    Message = "No event found",
                    Status = HttpStatusCode.OK,
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
                Data = events

            };
            var d1 = JsonConvert.SerializeObject(result1);
            var s1 = JObject.Parse(d1);
            return s1;
        }

        // PUT: api/Events/5
        [ResponseType(typeof(void))]
        [Authorize]
        public JObject PutEvents(int id, Events events)
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

            if (id != events.Id)
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

            db.Entry(events).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventsExists(id))
                {
                    var result1 = new ResponseModel
                    {
                        Message = "Not Found",
                        Status = HttpStatusCode.NotFound,
                        Data = null

                    };
                    var d1 = JsonConvert.SerializeObject(result1);
                    var s1 = JObject.Parse(d1);
                    return s1; ;
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

        //Check if User Storage is configured
        [ResponseType(typeof(string))]
        [Route("api/events/checkStorage")]
        public bool CheckStorage()
        {
            var currentUser = User.Identity.Name;
            var storage = db.Users.Where(x => x.UserName == currentUser).Select(x => x.AppParentFolderId).FirstOrDefault();
            if (storage == null || storage == string.Empty)
            {
                return false;

            }
            else
            {
                return true;
            }
        }

        // POST: api/Events
        //[ResponseType(typeof(Events))]
        [Authorize]
        public JObject PostEvents(Events events)
        {
            try
            {


                if (!CheckStorage())
                {
                    var err = "Storage not configured";
                    var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.Content = new StringContent(err.ToString(), Encoding.UTF8, "application/json");
                    var result = new ResponseModel
                    {
                        Message = "Storage not configured",
                        Status = HttpStatusCode.BadRequest,
                        Data = null

                    };
                    var d1 = JsonConvert.SerializeObject(result);
                    var s1 = JObject.Parse(d1);
                    return s1;
                }

                events.EndTime = null;
                events.CreatedBy = User.Identity.GetUserId();
                db.Events.Add(events);
                db.SaveChanges();

                if (events.GroupId == null || events.GroupId == 0)
                {
                    if (events.InvitedUsers != null)
                    {
                        GroupMaster group = new GroupMaster();
                        group.GroupName = events.EventName + "_Group";
                        group.IsUserCreated = false;
                        group.CreatedAt = DateTime.Now;
                        group.CreatedBy = User.Identity.GetUserId();
                        db.GroupsMasters.Add(group);
                        db.SaveChanges();

                        foreach (var item in events.InvitedUsers)
                        {
                            var user = db.Users.Where(x => x.UserName == item).ToList();
                            //if the user is already a registered member, display this invitation in the event list as Pending Invitation
                            if (user.Count > 0)
                            {
                                var _user = db.Users.Where(x => x.PhoneNumber == item).SingleOrDefault();
                                UserGroupMapping userGroup = new UserGroupMapping();
                                userGroup.GroupId = group.Id;
                                userGroup.UserName = _user.UserName;
                                userGroup.IsInviteAccepted = false;
                                db.UserGroupMappings.Add(userGroup);
                                db.SaveChanges();
                                //Push Notification
                            }
                            else
                            {
                                //If the user is not registered, send an SMS with the link of the app
                                InvitedUsers invited = new InvitedUsers();
                                invited.PhoneNumber = item;
                                invited.IsInvitationAccepted = false;
                                invited.EventId = events.Id;
                                db.InvitedUsers.Add(invited);
                                db.SaveChanges();
                                //SendSMS with applink

                                //
                            }
                        }

                        var eventGroup = new EventGroupMapping();
                        eventGroup.GroupId = group.Id;
                        eventGroup.EventId = events.Id;
                        db.EventGroupMapping.Add(eventGroup);
                        db.SaveChanges();
                        events.GroupId = group.Id;
                    }
                    // db.SaveChanges();
                }
                else
                {
                    var eventGroup = new EventGroupMapping();
                    eventGroup.GroupId = events.GroupId.Value;
                    eventGroup.EventId = events.Id;
                    db.EventGroupMapping.Add(eventGroup);
                    db.SaveChanges();
                    events.GroupId = events.GroupId.Value;
                }

                //Create eventFolder
                var id = User.Identity.GetUserId();
                var ParentFolderId = UserManager.Users.Where(x => x.Id == id).Select(x => x.AppParentFolderId).FirstOrDefault();
                HomeController home = new HomeController();
                var eventStatus = home.CreateEventFolder(id, ParentFolderId, events.Id);
                var updatedEvent = new Events();
                if (eventStatus == "Success")
                {
                    updatedEvent = db.Events.Find(events.Id);
                }
                else
                {
                    var result3 = new ResponseModel
                    {
                        Message = "Access Token expired.",
                        Status = HttpStatusCode.BadRequest,
                        Data = null

                    };
                    var d3 = JsonConvert.SerializeObject(result3);
                    var s3 = JObject.Parse(d3);
                    return s3;
                }
               
                //Share Folder with Collaborators
                var groupUsers = db.UserGroupMappings.Where(x => x.GroupId == events.GroupId).Select(x => x.UserName).ToList();
                var userEmails = new List<string>();
                if (groupUsers.Count > 0)
                {
                    foreach (var item in groupUsers)
                    {
                        var emails = UserManager.Users.Where(x => x.UserName == item).Select(x => x.Email).FirstOrDefault();
                        userEmails.Add(emails);
                    }
                    var share = home.ShareFolder(ParentFolderId, userEmails, id);
                }

                var result1 = new ResponseModel
                {
                    Message = "eventStatus",
                    Status = HttpStatusCode.OK,
                    Data = updatedEvent

                };
                var d = JsonConvert.SerializeObject(result1);
                var s = JObject.Parse(d);
                return s;
            }
            catch (Exception ex)
            {
                var result2 = new ResponseModel
                {
                    Message = ex.ToString(),
                    Status = HttpStatusCode.BadRequest,
                    Data = null

                };
                var d1 = JsonConvert.SerializeObject(result2);
                var s1 = JObject.Parse(d1);
                return s1;

            }
        }


        //Check invitations for event
        [Route("api/events/CheckInvitations")]
        [Authorize]
        public JObject CheckInvitations(int eventId)
        {
            var invitations = db.InvitedUsers.Where(x => x.EventId == eventId).ToList();
            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = invitations

            };
            var d = JsonConvert.SerializeObject(result);
            var s = JObject.Parse(d);
            return s;
            //return invitations;
        }

        // DELETE: api/Events/5
        // [ResponseType(typeof(Events))]
        [Authorize]
        public JObject DeleteEvents(int id)
        {
            Events events = db.Events.Find(id);
            if (events == null)
            {
                var result1 = new ResponseModel
                {
                    Message = "Success",
                    Status = HttpStatusCode.OK,
                    Data = 0

                };
                var d1 = JsonConvert.SerializeObject(result1);
                var s1 = JObject.Parse(d1);
                return s1;
            }

            db.Events.Remove(events);
            db.SaveChangesAsync();

            var result = new ResponseModel
            {
                Message = "Success",
                Status = HttpStatusCode.OK,
                Data = 0

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

        private bool EventsExists(int id)
        {
            return db.Events.Count(e => e.Id == id) > 0;
        }


    }
}