namespace mobileshopping.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }
}
