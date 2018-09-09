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
    public class UserGroupMappingsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/UserGroupMappings
        public IQueryable<UserGroupMapping> GetUserGroupMappings()
        {
            return db.UserGroupMappings;
        }

        // GET: api/UserGroupMappings/5
        [ResponseType(typeof(UserGroupMapping))]
        public async Task<IHttpActionResult> GetUserGroupMapping(int id)
        {
            UserGroupMapping userGroupMapping = await db.UserGroupMappings.FindAsync(id);
            if (userGroupMapping == null)
            {
                return NotFound();
            }

            return Ok(userGroupMapping);
        }

        // PUT: api/UserGroupMappings/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUserGroupMapping(int id, UserGroupMapping userGroupMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != userGroupMapping.Id)
            {
                return BadRequest();
            }

            db.Entry(userGroupMapping).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserGroupMappingExists(id))
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

        // POST: api/UserGroupMappings
        [ResponseType(typeof(UserGroupMapping))]
        public async Task<IHttpActionResult> PostUserGroupMapping(UserGroupMapping userGroupMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.UserGroupMappings.Add(userGroupMapping);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = userGroupMapping.Id }, userGroupMapping);
        }

        // DELETE: api/UserGroupMappings/5
        [ResponseType(typeof(UserGroupMapping))]
        public async Task<IHttpActionResult> DeleteUserGroupMapping(int id)
        {
            UserGroupMapping userGroupMapping = await db.UserGroupMappings.FindAsync(id);
            if (userGroupMapping == null)
            {
                return NotFound();
            }

            db.UserGroupMappings.Remove(userGroupMapping);
            await db.SaveChangesAsync();

            return Ok(userGroupMapping);
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