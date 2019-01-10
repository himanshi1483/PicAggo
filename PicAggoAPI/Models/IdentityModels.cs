using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PicAggoAPI.Providers;

namespace PicAggoAPI.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string DefaultStorage { get; set; }
        public string AppParentFolderId { get; set; }
                                           //  public string RefreshToken { get; set; }
        public string DeviceToken { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public DbSet<StoredResponse> StoredResponse { get; set; }
        public DbSet<GroupMaster> GroupsMasters { get; set; }
        public DbSet<UserGroupMapping> UserGroupMappings { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<EventDetails> EventDetails { get; set; }
        public DbSet<LogMetadata> LogMetadata { get; set; }
        public DbSet<EventGroupMapping> EventGroupMapping { get; set; }
        public DbSet<InvitedUsers> InvitedUsers { get; set; }
      //  public DbSet<GoogleUserCredential> GoogleUserCredential { get; set; }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}