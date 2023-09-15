using ECommerce.API.Models;
using System.Data.SqlClient;

namespace ECommerce.API.DataAccess
{
    public interface IDataAccess
    {
        List<ProductCategory> GetProductCategories();
        ProductCategory GetProductCategory(int id);
        Offer GetOffer(int id);
        public List<Product> GetProductsAll();
        List<Product> GetProducts(string category, string subcategory, int count); 
        Product GetProduct(int id);
        public void UpdateActiveCartOfUser(int userId, List<CartItem> updatedCartItems);
        public bool UpdateCartItemDuration(int userId, int cartId, int cartItemId, int updatedDuration);
        //public int GetActiveCartIdForUser(int userId, SqlConnection connection, SqlTransaction transaction);
        public void ClearCartItems(int cartId, SqlConnection connection, SqlTransaction transaction);
        public void AddCartItems(int cartId, List<CartItem> cartItems, SqlConnection connection, SqlTransaction transaction);
        Product CreateProduct(Product product);
        public List<User> GetAllUsers();
        public Cart GetActiveCartOfUser1(int userId, SqlConnection connection, SqlTransaction transaction);
        Product UpdateProduct(int id, Product product);
        void DeleteProduct(int id);
        bool InsertUser(User user);
        string IsUserPresent(string email, string password);
        void InsertReview(Review review);
        List<Review> GetProductReviews(int productId);
        User GetUser(int id);
        bool InsertCartItem(int userId, int productId, int Duration);
        Cart GetActiveCartOfUser(int userid);
        Cart GetCart(int cartid);
        List<Cart> GetAllPreviousCartsOfUser(int userid);
        List<PaymentMethod> GetPaymentMethods();
        int InsertPayment(Payment payment);
        int InsertOrder(Order order);
    }
}
