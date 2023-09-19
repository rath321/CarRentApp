using ECommerce.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication_13.Models
{
    public class ApplicationDbContext:IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedRoles(builder);
        }
        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" },
                new IdentityRole() { Name = "HR", ConcurrencyStamp = "3", NormalizedName = "HR" });
        }
        //private void SeedUsers(ModelBuilder builder)
        //{
        //    var hasher = new PasswordHasher<IdentityUser>();
        //    builder.Entity<IdentityUser>().HasData(
        //        new ApplicationUser
        //        {
        //            Id = "1", 
        //            UserName = "admin@example.com",
        //            NormalizedUserName = "ADMIN@EXAMPLE.COM",
        //            Email = "admin@example.com",
        //            NormalizedEmail = "ADMIN@EXAMPLE.COM",
        //            EmailConfirmed = true,
        //            PasswordHash = hasher.HashPassword(null, "YourAdminPasswordHere"), 
        //            SecurityStamp = string.Empty
        //        },
        //        new ApplicationUser
        //        {
        //            Id = "2", 
        //            UserName = "user@example.com",
        //            NormalizedUserName = "USER@EXAMPLE.COM",
        //            Email = "user@example.com",
        //            NormalizedEmail = "USER@EXAMPLE.COM",
        //            EmailConfirmed = true,
        //            PasswordHash = hasher.HashPassword(null, "YourUserPasswordHere"), 
        //            SecurityStamp = string.Empty
        //        }
        //    );

            
        //    builder.Entity<IdentityUserRole<string>>().HasData(
        //        new IdentityUserRole<string>
        //        {
        //            UserId = "1", // Admin user Id
        //            RoleId = "1" // Admin role Id
        //        },
        //        new IdentityUserRole<string>
        //        {
        //            UserId = "2", // Regular user Id
        //            RoleId = "2" // User role Id
        //        }
        //    );
        //}
        public DbSet<Product> Products { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
    }
}
