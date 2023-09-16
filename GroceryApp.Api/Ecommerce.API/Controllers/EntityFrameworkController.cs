using Ecommerce.API.DataAccess;
using Ecommerce.API.Models;
using ECommerce.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityFrameworkController : ControllerBase
    {
        public readonly MyDbContext _dbContext;
        public EntityFrameworkController(MyDbContext dbContext) {
        _dbContext= dbContext;
        }
        [HttpPost("CreateProduct")]
        public IActionResult CreateProduct( ProductModel product)
        { 
            try
            {
                // Add the product to the DbContext and save changes
                _dbContext.ProductsModel.Add(product);
                _dbContext.SaveChanges();

                // Return the created product with its generated ID
                return Ok(product);
    }
            catch (Exception ex)
            {
                // Log or return the inner exception's message for debugging
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
}
        }




    }
}
