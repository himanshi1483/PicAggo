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
using PicAggoAPI.Models;

namespace PicAggoAPI.Controllers
{
    public class EventDetailsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/EventDetails
        public IQueryable<EventDetails> GetEventDetails()
        {
            return db.EventDetails;
        }

        // GET: api/EventDetails/5
        [ResponseType(typeof(EventDetails))]
        public async Task<IHttpActionResult> GetEventDetails(int id)
        {
            EventDetails eventDetails = await db.EventDetails.FindAsync(id);
            if (eventDetails == null)
            {
                return NotFound();
            }

            return Ok(eventDetails);
        }

        // PUT: api/EventDetails/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEventDetails(int id, EventDetails eventDetails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != eventDetails.Id)
            {
                return BadRequest();
            }

            db.Entry(eventDetails).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventDetailsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/EventDetails
        [ResponseType(typeof(EventDetails))]
        public async Task<IHttpActionResult> PostEventDetails(EventDetails eventDetails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.EventDetails.Add(eventDetails);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = eventDetails.Id }, eventDetails);
        }

        // DELETE: api/EventDetails/5
        [ResponseType(typeof(EventDetails))]
        public async Task<IHttpActionResult> DeleteEventDetails(int id)
        {
            EventDetails eventDetails = await db.EventDetails.FindAsync(id);
            if (eventDetails == null)
            {
                return NotFound();
            }

            db.EventDetails.Remove(eventDetails);
            await db.SaveChangesAsync();

            return Ok(eventDetails);
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