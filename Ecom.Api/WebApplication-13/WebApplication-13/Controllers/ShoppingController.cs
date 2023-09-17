using Ecommerce.API.Models;
using ECommerce.API.DataAccess;
using ECommerce.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        readonly IDataAccess dataAccess;
        private readonly string DateFormat;
        public ShoppingController(IDataAccess dataAccess, IConfiguration configuration)
        {
            this.dataAccess = dataAccess;
            DateFormat = configuration["Constants:DateFormat"];
        }
       
        [HttpGet("GetCategoryList")]
        public IActionResult GetCategoryList()
        {
            var result = dataAccess.GetProductCategories();
            return Ok(result);
        }
        
        [HttpGet("GetProducts")]
        public IActionResult GetProducts(string category, string subcategory, int count)
        {
            var result = dataAccess.GetProducts(category, subcategory, count);
            return Ok(result);
        }
        [HttpGet("GetProductsAll")]
        public IActionResult GetProductsAll()
        {
            var result = dataAccess.GetProductsAll();
            return Ok(result);
        }
        [HttpGet("GetProduct/{id}")]
        public IActionResult GetProduct(int id)
        {
            var result = dataAccess.GetProduct(id);
            return Ok(result);
        }
        [Authorize(Roles="Admin")]
        [HttpPost("CreateProduct")]
        public IActionResult CreateProduct(Product product)
        {
            // Perform validation and error handling if needed
            // Assuming the Product object is passed in the request body

            var createdProduct = dataAccess.CreateProduct(product);

            // Return the created product
            return Ok(createdProduct);
        }
        [HttpPost("ToBeDeleted/{cartItemId}/{cartId}")]
        public IActionResult ToBeDeleted(int cartItemId, int cartId)
        {
            // Perform validation and error handling if needed
            // Assuming the Product object is passed in the request body

            dataAccess.InsertToBeDeletedItem(cartId, cartItemId);

            // Return the created product
            return Ok();
        }
        [HttpGet("ToBeDeleted")]
        public IActionResult ToBeDeletedGet(int cartItemId, int cartId)
        {
            // Perform validation and error handling if needed
            // Assuming the Product object is passed in the request body

            List<toBeDeleted> tmp = dataAccess.GetAllToBeDeletedItems();

            // Return the created product
            return Ok(tmp);
        }
        [HttpDelete("DeleteToBeDeleted/{cartItemId}/{cartId}")]
        public IActionResult DeleteToBeDeleted(int cartItemId, int cartId)
        {
            // Perform validation and error handling if needed
            // Assuming the Product object is passed in the request body

            dataAccess.DeleteToBeDeletedItem(cartId, cartItemId);

            // Return the created product
            return Ok();
        }
        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            // Perform validation and error handling if needed
            // Assuming the Product object is passed in the request body

            List<User> tmp = dataAccess.GetAllUsers();

            // Return the created product
            return Ok(tmp);
        }
        [HttpPut("UpdateProduct/{id}")]
        public IActionResult UpdateProduct(int id, Product product)
        {
            // Perform validation and error handling if needed
            // Assuming the Product object is passed in the request body

            var updatedProduct = dataAccess.UpdateProduct(id, product);

            // Return the updated product
            return Ok(updatedProduct);
        }

        [HttpDelete("DeleteProduct/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            dataAccess.DeleteProduct(id);

            // Return a success response
            return Ok();
        }
        [HttpDelete("DeleteCartItem/{cartItemId}/{cartId}/")]
        public IActionResult DeleteCartItem(int cartItemId, int cartId)
        {
            dataAccess.DeleteCartItem(cartId, cartItemId);

            // Return a success response
            return Ok();
        }
        [HttpGet("GetAllUsersCartItems")]
        public IActionResult GetAllUsersCartItems()
        {

            return Ok();
        }
        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser([FromBody] User user)
        {
            user.CreatedAt = DateTime.Now.ToString(DateFormat);
            user.ModifiedAt = DateTime.Now.ToString(DateFormat);

            var result = dataAccess.InsertUser(user);

            string? message;
            if (result) message = "inserted";
            else message = "email not available";
            return Ok(message);
        }

        [HttpPost("LoginUser")]
        public IActionResult LoginUser([FromBody] User user)
        {
            var token = dataAccess.IsUserPresent(user.Email, user.Password);
            if (token == "") token = "invalid";
            return Ok(token);
        }

        [HttpPost("InsertReview")]
        public IActionResult InsertReview([FromBody] Review review)
        {
            review.CreatedAt = DateTime.Now.ToString(DateFormat);
            dataAccess.InsertReview(review);
            return Ok("inserted");
        }

        [HttpGet("GetProductReviews/{productId}")]
        public IActionResult GetProductReviews(int productId)
        {
            var result = dataAccess.GetProductReviews(productId);
            return Ok(result);
        }

        [HttpPost("InsertCartItem/{userid}/{productid}/{Duration}")]
        public IActionResult InsertCartItem(int userid, int productid, int Duration)
        {
            var result = dataAccess.InsertCartItem(userid, productid, Duration);
            return Ok(result ? "inserted" : "not inserted");
        }


        [HttpPut("UpdateActiveCartOfUser/{id}")]
        public IActionResult UpdateActiveCartOfUser(int id, [FromBody] List<CartItem> updatedCartItems)
        {
            try
            {
                dataAccess.UpdateActiveCartOfUser(id, updatedCartItems);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("GetActiveCartOfUser/{id}")]
        public IActionResult GetActiveCartOfUser(int id)
        {
            var result = dataAccess.GetActiveCartOfUser(id);
            return Ok(result);
        }

        [HttpGet("GetAllPreviousCartsOfUser/{id}")]
        public IActionResult GetAllPreviousCartsOfUser(int id)
        {
            var result = dataAccess.GetAllPreviousCartsOfUser(id);
            return Ok(result);
        }
        [HttpPut("UpdateCartItemDuration/{userId}/{updatedDuration}")]
        public IActionResult updateCartItemDuration(int userId, int cartId, int cartItemId, int updatedDuration)
        {
            return Ok();
        }
        [HttpGet("GetPaymentMethods")]
        public IActionResult GetPaymentMethods()
        {
            var result = dataAccess.GetPaymentMethods();
            return Ok(result);
        }

        [HttpPost("InsertPayment")]
        public IActionResult InsertPayment(Payment payment)
        {
            payment.CreatedAt = DateTime.Now.ToString();
            var id = dataAccess.InsertPayment(payment);
            return Ok(id.ToString());
        }

        [HttpPost("InsertOrder")]
        public IActionResult InsertOrder(Order order)
        {
            order.CreatedAt = DateTime.Now.ToString();
            var id = dataAccess.InsertOrder(order);
            return Ok(id.ToString());
        }
    }
}
