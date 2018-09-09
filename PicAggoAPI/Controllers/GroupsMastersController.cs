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
    public class GroupsMastersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/GroupsMasters
        public IQueryable<GroupsMaster> GetGroupsMasters()
        {
            return db.GroupsMasters;
        }

        // GET: api/GroupsMasters/5
        [ResponseType(typeof(GroupsMaster))]
        public async Task<IHttpActionResult> GetGroupsMaster(int id)
        {
            GroupsMaster groupsMaster = await db.GroupsMasters.FindAsync(id);
            if (groupsMaster == null)
            {
                return NotFound();
            }

            return Ok(groupsMaster);
        }

        // PUT: api/GroupsMasters/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutGroupsMaster(int id, GroupsMaster groupsMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != groupsMaster.Id)
            {
                return BadRequest();
            }

            db.Entry(groupsMaster).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupsMasterExists(id))
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

        // POST: api/GroupsMasters
        [ResponseType(typeof(GroupsMaster))]
        public async Task<IHttpActionResult> PostGroupsMaster(GroupsMaster groupsMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.GroupsMasters.Add(groupsMaster);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = groupsMaster.Id }, groupsMaster);
        }

        // DELETE: api/GroupsMasters/5
        [ResponseType(typeof(GroupsMaster))]
        public async Task<IHttpActionResult> DeleteGroupsMaster(int id)
        {
            GroupsMaster groupsMaster = await db.GroupsMasters.FindAsync(id);
            if (groupsMaster == null)
            {
                return NotFound();
            }

            db.GroupsMasters.Remove(groupsMaster);
            await db.SaveChangesAsync();

            return Ok(groupsMaster);
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