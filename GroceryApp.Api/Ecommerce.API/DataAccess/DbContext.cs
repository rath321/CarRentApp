using Ecommerce.API.Models;
using ECommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.DataAccess
{
   
    public class MyDbContext : DbContext
    {
        public DbSet<ProductModel> ProductsModel { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }

        // Your constructor with options
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public MyDbContext()
        {
        }
    }

}
