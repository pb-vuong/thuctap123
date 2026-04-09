namespace mobileshopping.Models
{
    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; } // Lưu tên tại thời điểm mua
        public decimal Price { get; set; }       // Lưu giá tại thời điểm mua
        public int Quantity { get; set; }
    }
}
