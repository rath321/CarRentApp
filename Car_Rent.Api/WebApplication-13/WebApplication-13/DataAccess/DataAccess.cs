
using ECommerce.API.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data.Common;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Ecommerce.API.Models;
using Microsoft.Data.SqlClient;

namespace ECommerce.API.DataAccess
{
    public class DataAccess : IDataAccess
    {
        private readonly IConfiguration configuration;
        private readonly string dbconnection;
        private readonly string dateformat;

        public DataAccess(IConfiguration configuration)
        {
            this.configuration = configuration;
            dbconnection = this.configuration["ConnectionStrings:DB"];
            dateformat = this.configuration["Constants:DateFormat"];
        }

        public Cart GetActiveCartOfUser(int userid)
        {
            var cart = new Cart();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand()
                {
                    Connection = connection
                };
                connection.Open();

                try
                {
                    string query = "SELECT COUNT(*) FROM Carts WHERE UserId=" + userid + " AND Ordered='false';";
                    command.CommandText = query;

                    int count = (int)command.ExecuteScalar();
                    if (count == 0)
                    {
                        return cart;
                    }

                    query = "SELECT CartId From Carts WHERE UserId=" + userid + " AND Ordered='false';";
                    command.CommandText = query;

                    int cartid = (int)command.ExecuteScalar();

                    query = "select * from CartItems where CartId=" + cartid + ";";
                    command.CommandText = query;

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CartItem item = new CartItem()
                        {
                            Id = (int)reader["CartItemId"],
                            Product = GetProduct((int)reader["ProductId"]),
                            Duration = reader["Duration"] == DBNull.Value ? 0 : (int)reader["Duration"]
                        };
                        cart.CartItems.Add(item);
                    }

                    cart.Id = cartid;
                    cart.User = GetUser(userid);
                    cart.Ordered = false;
                    cart.OrderedOn = "";
                }
                catch (Exception ex)
                {
                    // Handle the exception here, e.g., log it or throw a custom exception.
                    // You can also return a default cart or null if an error occurs.
                    Console.WriteLine("An error occurred: " + ex.Message);
                    return null;
                }
            }
            return cart;
        }



        public void UpdateActiveCartOfUser(int userId, List<CartItem> updatedCartItems)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Get the active cart for the user
                        Cart activeCart = GetActiveCartOfUser1(userId, connection, transaction);

                        // Clear existing cart items
                        ClearCartItems(activeCart.Id, connection, transaction);

                        // Add the updated cart items
                        AddCartItems(activeCart.Id, updatedCartItems, connection, transaction);

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Handle any exception and rollback the transaction if necessary
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }

        public Cart GetActiveCartOfUser1(int userId, SqlConnection connection, SqlTransaction transaction)
        {
            var cart = new Cart();
            SqlCommand command = new SqlCommand()
            {
                Connection = connection,
                Transaction = transaction
            };

            string query = "SELECT COUNT(*) From Carts WHERE UserId=" + userId + " AND Ordered='false';";
            command.CommandText = query;

            int count = (int)command.ExecuteScalar();
            if (count == 0)
            {
                return cart;
            }

            query = "SELECT CartId From Carts WHERE UserId=" + userId + " AND Ordered='false';";
            command.CommandText = query;

            int cartId = (int)command.ExecuteScalar();

            query = "select * from CartItems where CartId=" + cartId + ";";
            command.CommandText = query;

            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int durationFromDB = reader["Duration"] == DBNull.Value ? 0 : (int)reader["Duration"];
                Console.WriteLine($"Duration from DB: {durationFromDB}");
                CartItem item = new CartItem()
                {
                    Id = (int)reader["CartItemId"],
                    Product = GetProduct((int)reader["ProductId"]),
                    Duration = durationFromDB
                };
                cart.CartItems.Add(item);
            }
            reader.Close();

            cart.Id = cartId;
            cart.User = GetUser(userId);
            cart.Ordered = false;
            cart.OrderedOn = "";

            return cart;
        }

        public void ClearCartItems(int cartId, SqlConnection connection, SqlTransaction transaction)
        {
            string query = "DELETE FROM CartItems WHERE CartId = @CartId;";
            SqlCommand command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@CartId", cartId);

            command.ExecuteNonQuery();
        }

        public void AddCartItems(int cartId, List<CartItem> cartItems, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                string query = "INSERT INTO CartItems (CartId, ProductId, Duration) VALUES (@CartId, @ProductId, @Duration);";
                SqlCommand command = new SqlCommand(query, connection, transaction);

                foreach (CartItem cartItem in cartItems)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@CartId", cartId);
                    command.Parameters.AddWithValue("@ProductId", cartItem.Product.Id);
                    command.Parameters.AddWithValue("@Duration", cartItem.Duration);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, you can log it or take appropriate action.
                Console.WriteLine("An error occurred: " + ex.Message);
                // You can also throw the exception again if needed.
                throw;
            }
        }


        public List<Cart> GetAllPreviousCartsOfUser(int userid)
        {
            var carts = new List<Cart>();
            try
            {
                using (SqlConnection connection = new SqlConnection(dbconnection))
                {
                    SqlCommand command = new SqlCommand()
                    {
                        Connection = connection
                    };
                    string query = "SELECT CartId FROM Carts WHERE UserId=" + userid + " AND Ordered='true';";
                    command.CommandText = query;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var cartid = (int)reader["CartId"];
                        carts.Add(GetCart(cartid));
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, you can log it or take appropriate action.
                Console.WriteLine("An error occurred: " + ex.Message);
                // You can also throw the exception again if needed.
                throw;
            }
            return carts;
        }


        public Cart GetCart(int cartid)
        {
            var cart = new Cart();
            try
            {
                using (SqlConnection connection = new SqlConnection(dbconnection))
                {
                    SqlCommand command = new SqlCommand()
                    {
                        Connection = connection
                    };
                    connection.Open();

                    string query = "SELECT * FROM CartItems WHERE CartId=" + cartid + ";";
                    command.CommandText = query;

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        CartItem item = new CartItem()
                        {
                            Id = (int)reader["CartItemId"],
                            Product = GetProduct((int)reader["ProductId"]),
                            Duration = (int)reader["Duration"]
                        };
                        cart.CartItems.Add(item);
                    }
                    reader.Close();

                    query = "SELECT * FROM Carts WHERE CartId=" + cartid + ";";
                    command.CommandText = query;
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        cart.Id = cartid;
                        cart.User = GetUser((int)reader["UserId"]);
                        cart.Ordered = bool.Parse((string)reader["Ordered"]);
                        cart.OrderedOn = (string)reader["OrderedOn"];
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, you can log it or take appropriate action.
                Console.WriteLine("An error occurred: " + ex.Message);
                // You can also throw the exception again if needed.
                throw;
            }
            return cart;
        }

        public bool UpdateCartItemDuration(int userId, int cartId, int cartItemId, int updatedDuration)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;

                    // Check if the specified cart item belongs to the given user and cart
                    string checkQuery = "SELECT COUNT(*) FROM CartItems WHERE CartItemId = @cartItemId AND CartId = @cartId;";
                    command.CommandText = checkQuery;
                    command.Parameters.AddWithValue("@cartItemId", cartItemId);
                    command.Parameters.AddWithValue("@cartId", cartId);

                    int cartItemCount = (int)command.ExecuteScalar();

                    if (cartItemCount == 1)
                    {
                        // Update the duration of the specified cart item
                        string updateQuery = "UPDATE CartItems SET Duration = @updatedDuration WHERE CartItemId = @cartItemId;";
                        command.CommandText = updateQuery;
                        command.Parameters.AddWithValue("@updatedDuration", updatedDuration);

                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected == 1; // Check if one row was updated
                    }
                    else
                    {
                        // The cart item does not belong to the specified user and cart
                        return false;
                    }
                }
            }
        }


        public Offer GetOffer(int id)
        {
            var offer = new Offer();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Offers WHERE OfferId=" + id + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader r = command.ExecuteReader();
                while (r.Read())
                {
                    offer.Id = (int)r["OfferId"];
                    offer.Title = (string)r["Title"];
                    offer.Discount = (int)r["Discount"];
                }
            }
            return offer;
        }

        public List<PaymentMethod> GetPaymentMethods()
        {
            var result = new List<PaymentMethod>();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM PaymentMethods;";
                command.CommandText = query;

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PaymentMethod paymentMethod = new()
                    {
                        Id = (int)reader["PaymentMethodId"],
                        Type = (string)reader["Type"],
                        Provider = (string)reader["Provider"],
                        Available = bool.Parse((string)reader["Available"]),
                        Reason = (string)reader["Reason"]
                    };
                    result.Add(paymentMethod);
                }
            }
            return result;
        }

        public Product GetProduct(int id)
        {
            var product = new Product();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Products WHERE ProductId=" + id + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    product.Id = (int)reader["ProductId"];
                    product.Title = (string)reader["Title"];
                    product.Description = (string)reader["Description"];
                    product.Price = (double)reader["Price"];
                    product.Quantity = (int)reader["Quantity"];
                    product.ImageName = (string)reader["ImageName"];

                    var categoryid = (int)reader["CategoryId"];
                    product.ProductCategory = GetProductCategory(categoryid);

                    var offerid = (int)reader["OfferId"];
                    product.Offer = GetOffer(offerid);
                }
            }
            return product;
        }

        public List<ProductCategory> GetProductCategories()
        {
            var productCategories = new List<ProductCategory>();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                string query = "SELECT * FROM ProductCategories;";
                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var category = new ProductCategory()
                    {
                        Id = (int)reader["CategoryId"],
                        Category = (string)reader["Category"],
                        SubCategory = (string)reader["SubCategory"]
                    };
                    productCategories.Add(category);
                }
            }
            return productCategories;
        }
       
        public Product CreateProduct(Product product)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand()
                {
                    Connection = connection
                };

                // Set up the SQL query with parameters
                string query = "INSERT INTO Products (Title, Description, Price, Quantity, ImageName, CategoryId, OfferId) " +
                               "VALUES (@Title, @Description, @Price, @Quantity, @ImageName, @CategoryId, @OfferId);" +
                               "SELECT SCOPE_IDENTITY();";
                command.CommandText = query;

                // Set the parameter values
                command.Parameters.AddWithValue("@Title", product.Title);
                command.Parameters.AddWithValue("@Description", product.Description);
                command.Parameters.AddWithValue("@Price", product.Price);
                command.Parameters.AddWithValue("@Quantity", product.Quantity);
                command.Parameters.AddWithValue("@ImageName", product.ImageName);
                command.Parameters.AddWithValue("@CategoryId", product.ProductCategory.Id);
                command.Parameters.AddWithValue("@OfferId", product.Offer.Id);

                connection.Open();
                var createdProductId = command.ExecuteScalar();

                product.Id = Convert.ToInt32(createdProductId);
            }

            return product;
        }

        public Product UpdateProduct(int id, Product product)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand()
                {
                    Connection = connection
                };

                // Set up the SQL query with parameters
                string query = "UPDATE Products SET Title=@Title, Description=@Description, Price=@Price, " +
                               "Quantity=@Quantity, ImageName=@ImageName, CategoryId=@CategoryId, OfferId=@OfferId " +
                               "WHERE ProductId=" + id + ";";
                command.CommandText = query;

                // Set the parameter values
                command.Parameters.AddWithValue("@Title", product.Title);
                command.Parameters.AddWithValue("@Description", product.Description);
                command.Parameters.AddWithValue("@Price", product.Price);
                command.Parameters.AddWithValue("@Quantity", product.Quantity);
                command.Parameters.AddWithValue("@ImageName", product.ImageName);
                command.Parameters.AddWithValue("@CategoryId", product.ProductCategory.Id);
                command.Parameters.AddWithValue("@OfferId", product.Offer.Id);
                command.Parameters.AddWithValue("@ProductId", id);

                connection.Open();
                command.ExecuteNonQuery();
            }

            return product;
        }

        public void DeleteProduct(int id)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand()
                {
                    Connection = connection
                };

                // Set up the SQL query with parameters
                string query = "DELETE FROM Products WHERE ProductId=" + id + ";";
                command.CommandText = query;

                // Set the parameter value
                command.Parameters.AddWithValue("@ProductId", id);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        public ProductCategory GetProductCategory(int id)
        {
            var productCategory = new ProductCategory();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM ProductCategories WHERE CategoryId=" + id + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader r = command.ExecuteReader();
                while (r.Read())
                {
                    productCategory.Id = (int)r["CategoryId"];
                    productCategory.Category = (string)r["Category"];
                    productCategory.SubCategory = (string)r["SubCategory"];
                }
            }

            return productCategory;
        }

        public List<Review> GetProductReviews(int productId)
        {
            var reviews = new List<Review>();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Reviews WHERE ProductId=" + productId + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var review = new Review()
                    {
                        Id = (int)reader["ReviewId"],
                        Value = (string)reader["Review"],
                        CreatedAt = (string)reader["CreatedAt"]
                    };

                    var userid = (int)reader["UserId"];
                    review.User = GetUser(userid);

                    var productid = (int)reader["ProductId"];
                    review.Product = GetProduct(productid);

                    reviews.Add(review);
                }
            }
            return reviews;
        }

        public List<Product> GetProducts(string category, string subcategory, int count)
        {
            var products = new List<Product>();
            try
            {
                using (SqlConnection connection = new SqlConnection(dbconnection))
                {
                    SqlCommand command = new SqlCommand()
                    {
                        Connection = connection
                    };

                    string query = "SELECT TOP " + count + " * FROM Products WHERE CategoryId=(SELECT CategoryId FROM ProductCategories WHERE Category=@c AND SubCategory=@s) ORDER BY newid();";
                    command.CommandText = query;
                    command.Parameters.Add("@c", System.Data.SqlDbType.NVarChar).Value = category;
                    command.Parameters.Add("@s", System.Data.SqlDbType.NVarChar).Value = subcategory;

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var product = new Product()
                        {
                            Id = (int)reader["ProductId"],
                            Title = (string)reader["Title"],
                            Description = (string)reader["Description"],
                            Price = (double)reader["Price"],
                            Quantity = (int)reader["Quantity"],
                            ImageName = (string)reader["ImageName"]
                        };

                        var categoryid = (int)reader["CategoryId"];
                        product.ProductCategory = GetProductCategory(categoryid);

                        var offerid = (int)reader["OfferId"];
                        product.Offer = GetOffer(offerid);

                        products.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, you can log it or take appropriate action.
                Console.WriteLine("An error occurred: " + ex.Message);
                // You can also throw the exception again if needed.
                throw;
            }
            return products;
        }


        public void InsertToBeDeletedItem(int cartId, int cartItemId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();
                string insertQuery = "INSERT INTO toBeDeletedItems (cartId, cartItemId) VALUES (@CartId, @CartItemId)";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@CartId", cartId);
                    command.Parameters.AddWithValue("@CartItemId", cartItemId);
                    command.ExecuteNonQuery();
                }
            }

        }
        public bool DeleteToBeDeletedItem(int cartId, int cartItemId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM toBeDeletedItems WHERE cartId = @CartId AND cartItemId = @CartItemId;";
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@CartId", cartId);
                    command.Parameters.AddWithValue("@CartItemId", cartItemId);

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0; // Return true if at least one row was deleted
                }
            }
        }

        public List<toBeDeleted> GetAllToBeDeletedItems()
        {
            List<toBeDeleted> toBeDeletedItems = new List<toBeDeleted>();

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM toBeDeletedItems";

                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            toBeDeleted item = new toBeDeleted
                            {
                                cartId = (int)reader["cartId"],
                                cartItemId = (int)reader["cartItemId"]
                            };
                            toBeDeletedItems.Add(item);
                        }
                    }
                }
            }

            return toBeDeletedItems;
        }


        public List<Product> GetProducts1(string category, string subcategory, int count)
        {
            var products = new List<Product>();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT TOP " + count + " * FROM Products WHERE CategoryId=(SELECT CategoryId FROM ProductCategories WHERE Category=@c AND SubCategory=@s) ORDER BY newid();";
                command.CommandText = query;
                command.Parameters.Add("@c", System.Data.SqlDbType.NVarChar).Value = category;
                command.Parameters.Add("@s", System.Data.SqlDbType.NVarChar).Value = subcategory;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var product = new Product()
                    {
                        Id = (int)reader["ProductId"],
                        Title = (string)reader["Title"],
                        Description = (string)reader["Description"],
                        Price = (double)reader["Price"],
                        Quantity = (int)reader["Quantity"],
                        ImageName = (string)reader["ImageName"]
                    };

                    var categoryid = (int)reader["CategoryId"];
                    product.ProductCategory = GetProductCategory(categoryid);

                    var offerid = (int)reader["OfferId"];
                    product.Offer = GetOffer(offerid);

                    products.Add(product);
                }
            }
            return products;
        }
        public List<Product> GetProductsAll()
        {
            var products = new List<Product>();
            try
            {
                using (SqlConnection connection = new SqlConnection(dbconnection))
                {
                    SqlCommand command = new SqlCommand()
                    {
                        Connection = connection
                    };

                    string query = "SELECT * FROM Products ORDER BY newid();";
                    command.CommandText = query;

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var product = new Product()
                        {
                            Id = (int)reader["ProductId"],
                            Title = (string)reader["Title"],
                            Description = (string)reader["Description"],
                            Price = (double)reader["Price"],
                            Quantity = (int)reader["Quantity"],
                            ImageName = (string)reader["ImageName"]
                        };

                        products.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception here, you can log it or take appropriate action.
                Console.WriteLine("An error occurred: " + ex.Message);
                // You can also throw the exception again if needed.
                throw;
            }
            return products;
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Users;";
                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User user = new User
                    {
                        Id = (int)reader["UserId"],
                        FirstName = (string)reader["FirstName"],
                        LastName = (string)reader["LastName"],
                        Email = (string)reader["Email"],
                        Address = (string)reader["Address"],
                        Mobile = (string)reader["Mobile"],
                        Password = (string)reader["Password"],
                        CreatedAt = (string)reader["CreatedAt"],
                        ModifiedAt = (string)reader["ModifiedAt"]
                    };
                    users.Add(user);
                }
            }
            return users;
        }

        public User GetUser(int id)
        {
            var user = new User();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Users WHERE UserId=" + id + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    user.Id = (int)reader["UserId"];
                    user.FirstName = (string)reader["FirstName"];
                    user.LastName = (string)reader["LastName"];
                    user.Email = (string)reader["Email"];
                    user.Address = (string)reader["Address"];
                    user.Mobile = (string)reader["Mobile"];
                    user.Password = (string)reader["Password"];
                    user.CreatedAt = (string)reader["CreatedAt"];
                    user.ModifiedAt = (string)reader["ModifiedAt"];
                }
            }
            return user;
        }

        public bool InsertCartItem(int userId, int productId, int duration)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbconnection))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        // Check if there is an active cart for the user
                        string cartCheckQuery = "SELECT COUNT(*) FROM Carts WHERE UserId = @UserId AND Ordered = 'false';";

                        command.CommandText = cartCheckQuery;
                        command.Parameters.AddWithValue("@UserId", userId);

                        int cartCount = (int)command.ExecuteScalar();

                        if (cartCount == 0)
                        {
                            // If no active cart exists, create one
                            string createCartQuery = "INSERT INTO Carts (UserId, Ordered, OrderedOn) VALUES (@UserId, 'false', '');";

                            command.CommandText = createCartQuery;
                            command.ExecuteNonQuery();
                        }

                        // Get the CartId of the active cart
                        string getCartIdQuery = "SELECT CartId FROM Carts WHERE UserId = @UserId AND Ordered = 'false';";

                        command.CommandText = getCartIdQuery;

                        int cartId = (int)command.ExecuteScalar();

                        // Insert the new cart item
                        string insertCartItemQuery = "INSERT INTO CartItems (CartId, ProductId, Duration) VALUES (@CartId, @ProductId, @Duration);";

                        command.CommandText = insertCartItemQuery;
                        command.Parameters.AddWithValue("@CartId", cartId);
                        command.Parameters.AddWithValue("@ProductId", productId);
                        command.Parameters.AddWithValue("@Duration", duration);

                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Handle the exception here, you can log it or take appropriate action.
                Console.WriteLine("An error occurred: " + ex.Message);
                // You can also throw the exception again if needed.
                throw;
            }
        }


        public bool DeleteCartItem(int cartId, int cartItemId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    // Check if the cart item exists in the specified cart
                    string checkCartItemQuery = "SELECT COUNT(*) FROM CartItems WHERE CartId = @CartId AND CartItemId = @CartItemId;";

                    command.CommandText = checkCartItemQuery;
                    command.Parameters.AddWithValue("@CartId", cartId);
                    command.Parameters.AddWithValue("@CartItemId", cartItemId);

                    int cartItemCount = (int)command.ExecuteScalar();

                    if (cartItemCount == 1)
                    {
                        // If the cart item exists, delete it
                        string deleteCartItemQuery = "DELETE FROM CartItems WHERE CartId = @CartId AND CartItemId = @CartItemId;";

                        command.CommandText = deleteCartItemQuery;
                        command.ExecuteNonQuery();

                        return true; // Item deleted successfully
                    }
                    else
                    {
                        // If the cart item doesn't exist or there are multiple matching items (unexpected), return false or handle as needed
                        return false; // Item not found or unexpected situation
                    }
                }
            }
        }

        public int InsertOrder(Order order)
        {
            int value = 0;

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "INSERT INTO Orders (UserId, CartId, PaymentId, CreatedAt) values (@uid, @cid, @pid, @cat);";

                command.CommandText = query;
                command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = order.User.Id;
                command.Parameters.Add("@cid", System.Data.SqlDbType.Int).Value = order.Cart.Id;
                command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = order.CreatedAt;
                command.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = order.Payment.Id;

                connection.Open();
                value = command.ExecuteNonQuery();

                if (value > 0)
                {
                    query = "UPDATE Carts SET Ordered='true', OrderedOn='" + DateTime.Now.ToString(dateformat) + "' WHERE CartId=" + order.Cart.Id + ";";
                    command.CommandText = query;
                    command.ExecuteNonQuery();

                    query = "SELECT TOP 1 Id FROM Orders ORDER BY Id DESC;";
                    command.CommandText = query;
                    value = (int)command.ExecuteScalar();
                }
                else
                {
                    value = 0;
                }
            }

            return value;
        }

        public int InsertPayment(Payment payment)
        {
            int value = 0;
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = @"INSERT INTO Payments (PaymentMethodId, UserId, TotalAmount, ShippingCharges, AmountReduced, AmountPaid, CreatedAt) 
                                VALUES (@pmid, @uid, @ta, @sc, @ar, @ap, @cat);";

                command.CommandText = query;
                command.Parameters.Add("@pmid", System.Data.SqlDbType.Int).Value = payment.PaymentMethod.Id;
                command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = payment.User.Id;
                command.Parameters.Add("@ta", System.Data.SqlDbType.NVarChar).Value = payment.TotalAmount;
                command.Parameters.Add("@sc", System.Data.SqlDbType.NVarChar).Value = payment.ShipingCharges;
                command.Parameters.Add("@ar", System.Data.SqlDbType.NVarChar).Value = payment.AmountReduced;
                command.Parameters.Add("@ap", System.Data.SqlDbType.NVarChar).Value = payment.AmountPaid;
                command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = payment.CreatedAt;

                connection.Open();
                value = command.ExecuteNonQuery();

                if (value > 0)
                {
                    query = "SELECT TOP 1 Id FROM Payments ORDER BY Id DESC;";
                    command.CommandText = query;
                    value = (int)command.ExecuteScalar();
                }
                else
                {
                    value = 0;
                }
            }
            return value;
        }

        public void InsertReview(Review review)
        {
            using SqlConnection connection = new(dbconnection);
            SqlCommand command = new()
            {
                Connection = connection
            };

            string query = "INSERT INTO Reviews (UserId, ProductId, Review, CreatedAt) VALUES (@uid, @pid, @rv, @cat);";
            command.CommandText = query;
            command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = review.User.Id;
            command.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = review.Product.Id;
            command.Parameters.Add("@rv", System.Data.SqlDbType.NVarChar).Value = review.Value;
            command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = review.CreatedAt;

            connection.Open();
            command.ExecuteNonQuery();
        }

        public bool InsertUser(User user)
        {
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                connection.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE Email='" + user.Email + "';";
                command.CommandText = query;
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    connection.Close();
                    return false;
                }

                query = "INSERT INTO Users (FirstName, LastName, Address, Mobile, Email, Password, CreatedAt, ModifiedAt) values (@fn, @ln, @add, @mb, @em, @pwd, @cat, @mat);";

                command.CommandText = query;
                command.Parameters.Add("@fn", System.Data.SqlDbType.NVarChar).Value = user.FirstName;
                command.Parameters.Add("@ln", System.Data.SqlDbType.NVarChar).Value = user.LastName;
                command.Parameters.Add("@add", System.Data.SqlDbType.NVarChar).Value = user.Address;
                command.Parameters.Add("@mb", System.Data.SqlDbType.NVarChar).Value = user.Mobile;
                command.Parameters.Add("@em", System.Data.SqlDbType.NVarChar).Value = user.Email;
                command.Parameters.Add("@pwd", System.Data.SqlDbType.NVarChar).Value = user.Password;
                command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = user.CreatedAt;
                command.Parameters.Add("@mat", System.Data.SqlDbType.NVarChar).Value = user.ModifiedAt;

                command.ExecuteNonQuery();
            }
            return true;
        }


        public string IsUserPresent(string email, string password)
        {
            User user = new();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
                command.CommandText = query;
                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    connection.Close();
                    return "";
                }

                query = "SELECT * FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    user.Id = (int)reader["UserId"];
                    user.FirstName = (string)reader["FirstName"];
                    user.LastName = (string)reader["LastName"];
                    user.Email = (string)reader["Email"];
                    user.Address = (string)reader["Address"];
                    user.Mobile = (string)reader["Mobile"];
                    user.Password = (string)reader["Password"];
                    user.CreatedAt = (string)reader["CreatedAt"];
                    user.ModifiedAt = (string)reader["ModifiedAt"];
                }

                string key = "this is my custom Secret key for authentication";
                string duration = "60";
                var symmetrickey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(symmetrickey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("firstName", user.FirstName),
                    new Claim("lastName", user.LastName),
                    new Claim("address", user.Address),
                    new Claim("mobile", user.Mobile),
                    new Claim("email", user.Email),
                    new Claim("createdAt", user.CreatedAt),
                    new Claim("modifiedAt", user.ModifiedAt)
                };

                var jwtToken = new JwtSecurityToken(
                    issuer: "localhost",
                    audience: "localhost",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Int32.Parse(duration)),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }
            return "";
        }

      
    }
}
