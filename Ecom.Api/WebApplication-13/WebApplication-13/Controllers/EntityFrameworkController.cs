
using Ecommerce.API.Models;
using ECommerce.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication_13.Models;

namespace Ecommerce.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EntityFrameworkController : ControllerBase
    {
        public readonly ApplicationDbContext _dbContext;
        public EntityFrameworkController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [Authorize(Roles ="Admin")]
        [HttpPost("CreateProduct")]
        public IActionResult CreateProduct(Product product)
        {
            try
            {
                var existingOffer = _dbContext.Offers
    .FirstOrDefault(o => o.Id == product.Offer.Id);

                // Find the existing ProductCategory based on its properties
                var existingCategory = _dbContext.ProductCategories
                    .FirstOrDefault(c => c.Id == product.ProductCategory.Id);
                // Add the product to the DbContext and save changes
                product.Offer = existingOffer;
                product.ProductCategory = existingCategory;
                _dbContext.Products.Add(product);
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
        [Authorize(Roles ="Admin")]

        [HttpPut("UpdateProduct/{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            try
            {
                // Find the existing product by ID
                var existingProduct = _dbContext.Products.FirstOrDefault(p => p.Id == id);

                if (existingProduct == null)
                {
                    return NotFound("Product not found.");
                }

                // Update the existing product's properties with the new values
                existingProduct.Title = updatedProduct.Title;
                existingProduct.Description = updatedProduct.Description;
                existingProduct.Price = updatedProduct.Price;
                existingProduct.Quantity = updatedProduct.Quantity;
                existingProduct.ImageName = updatedProduct.ImageName;

                // Update the ProductCategory and Offer references if needed (similar to the create operation)
                var existingOffer = _dbContext.Offers.FirstOrDefault(o => o.Id == updatedProduct.Offer.Id);
                var existingCategory = _dbContext.ProductCategories.FirstOrDefault(c => c.Id == updatedProduct.ProductCategory.Id);

                existingProduct.Offer = existingOffer;
                existingProduct.ProductCategory = existingCategory;

                // Save the changes
                _dbContext.SaveChanges();

                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
            }
        }
        [HttpDelete("DeleteProduct/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                // Find the existing product by ID
                var existingProduct = _dbContext.Products.FirstOrDefault(p => p.Id == id);

                if (existingProduct == null)
                {
                    return NotFound("Product not found.");
                }

                // Remove the product from the DbContext
                _dbContext.Products.Remove(existingProduct);

                // Save the changes
                _dbContext.SaveChanges();

                return Ok("Product deleted successfully.");
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
            }
        }

        [HttpGet("GetProduct/{id}")]
        public IActionResult GetProduct(int id)
        {
            try
            {
                // Find the product by ID
                var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);

                // Find the existing ProductCategory based on its properties

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
            }
        }
        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts()
        {
            try
            {
                // Retrieve all products from the DbContext
                var products = _dbContext.Products.ToList();

                return Ok(products);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest(errorMessage);
            }
        }


    }

}
