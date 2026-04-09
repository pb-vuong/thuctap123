using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileshopping.Models;

namespace mobileshopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }
        //1. mua ngay 1 sản phẩm (hiện bill cho sản phẩm đó)
        [HttpPost("buy-now")]
        public async Task<ActionResult<Order>> BuyNow(int userId, int productId, int quantity)
        {
            var product = await
                _context.Products.FindAsync(productId);
            if (product == null) return NotFound("san pham khong ton tai");
            var subTotal = product.Price * quantity;
            var tax = subTotal * 0.1m;//gia su thuế 10%
            var order = new Order
            {
                UserID = userId,
                SubTotal = subTotal,
                Tax = tax,
                Total = subTotal + tax,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductID = productId,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        Quantity = quantity
                    }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return Ok(order);//tra ve noi dung hoa don (bill)
        }
        //2. mua các mặt hàng trong giỏ hàng
        [HttpPost("checkout-cart")]
        public async Task<ActionResult<Order>> CheckoutCart(int userId)
        {
            //lấy giỏ hàng và các item kèm thông tin sản phẩm
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserID == userId);
            if (cart == null) return NotFound("gia hang trống");
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartID == cart.CartID)
                .ToListAsync();
            if (!cartItems.Any()) return BadRequest("gio hang ko co san pham");
            var order = new Order
            {
                UserID = userId,
                SubTotal = cart.SubTotal,
                Tax = cart.Tax,
                Total = cart.Total
            };
            foreach (var item in cartItems)
            {
                var product = await
                _context.Products.FindAsync(item.ProductID);
                if (product != null)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductID = item.ProductID,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        Quantity = item.Quantity
                    });
                }
            }
            _context.Orders.Add(order);
            // Xóa giỏ hàng sau khi đã tạo đơn hàng thành công
            _context.CartItems.RemoveRange(cartItems);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return Ok(order);// trả về bill của cả giỏ hàng
        }
    }
}
