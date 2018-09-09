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
    public class EventGroupMappingsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/EventGroupMappings
        public IQueryable<EventGroupMapping> GetEventGroupMapping()
        {
            return db.EventGroupMapping;
        }

        // GET: api/EventGroupMappings/5
        [ResponseType(typeof(EventGroupMapping))]
        public async Task<IHttpActionResult> GetEventGroupMapping(int id)
        {
            EventGroupMapping eventGroupMapping = await db.EventGroupMapping.FindAsync(id);
            if (eventGroupMapping == null)
            {
                return NotFound();
            }

            return Ok(eventGroupMapping);
        }

        // PUT: api/EventGroupMappings/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEventGroupMapping(int id, EventGroupMapping eventGroupMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != eventGroupMapping.Id)
            {
                return BadRequest();
            }

            db.Entry(eventGroupMapping).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventGroupMappingExists(id))
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

        // POST: api/EventGroupMappings
        [ResponseType(typeof(EventGroupMapping))]
        public async Task<IHttpActionResult> PostEventGroupMapping(EventGroupMapping eventGroupMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.EventGroupMapping.Add(eventGroupMapping);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = eventGroupMapping.Id }, eventGroupMapping);
        }

        // DELETE: api/EventGroupMappings/5
        [ResponseType(typeof(EventGroupMapping))]
        public async Task<IHttpActionResult> DeleteEventGroupMapping(int id)
        {
            EventGroupMapping eventGroupMapping = await db.EventGroupMapping.FindAsync(id);
            if (eventGroupMapping == null)
            {
                return NotFound();
            }

            db.EventGroupMapping.Remove(eventGroupMapping);
            await db.SaveChangesAsync();

            return Ok(eventGroupMapping);
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