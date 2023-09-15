namespace ECommerce.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public UserCartItems User { get; set; } = new UserCartItems();
        public Cart Cart { get; set; } = new Cart();
        public Payment Payment { get; set; } = new Payment();
        public string CreatedAt { get; set; } = string.Empty;
    }
}
