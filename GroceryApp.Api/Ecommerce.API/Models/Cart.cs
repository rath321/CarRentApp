namespace ECommerce.API.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public UserCartItems User { get; set; } = new UserCartItems();
        public List<CartItem> CartItems { get; set; } = new();
        public bool Ordered { get; set; }
        public string OrderedOn { get; set; } = string.Empty;
    }
}
