using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileshopping.Models;
using mobileshopping.Models;

[Route("api/[controller]")]
[ApiController]
public class CartsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CartsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. LẤY GIỎ HÀNG CỦA USER (Hiển thị ở Frame 1)
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetCart(int userId)
    {
        // Tìm giỏ hàng của User này, kèm theo danh sách các món đồ (Items) bên trong
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserID == userId);

        if (cart == null) return NotFound("Người dùng này chưa có giỏ hàng.");

        // Lấy danh sách Item và thông tin sản phẩm (để lấy tên, ảnh, giá sản phẩm)
        var cartItems = await _context.CartItems
            .Where(ci => ci.CartID == cart.CartID)
            .Join(_context.Products,
                  item => item.ProductID,
                  prod => prod.ProductID,
                  (item, prod) => new {
                      item.CartItemID,
                      prod.ProductName,
                      prod.ImageURL,
                      prod.Price,
                      item.Quantity,
                      TotalItemPrice = prod.Price * item.Quantity
                  }).ToListAsync();

        // Tính toán lại SubTotal, Tax, Total trước khi trả về (Giống Frame 1)
        decimal subTotal = cartItems.Sum(x => x.TotalItemPrice);
        decimal tax = subTotal * 0.1m; // Giả sử thuế 10%
        decimal total = subTotal + tax;

        return Ok(new
        {
            cart.CartID,
            Items = cartItems,
            SubTotal = subTotal,
            Tax = tax,
            Total = total
        });
    }

    // 2. THÊM SẢN PHẨM VÀO GIỎ (Khi bấm nút ở Frame 3)
    [HttpPost("add-item")]
    public async Task<IActionResult> AddToCart(int userId, int productId, int quantity)
    {
        // B1: Tìm giỏ hàng của User, nếu chưa có thì tạo mới
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserID == userId);
        if (cart == null)
        {
            cart = new Cart { UserID = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // B2: Kiểm tra sản phẩm này đã có trong giỏ chưa
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartID == cart.CartID && ci.ProductID == productId);

        if (existingItem != null)
        {
            // Nếu có rồi thì tăng số lượng
            existingItem.Quantity += quantity;
        }
        else
        {
            // Nếu chưa có thì thêm mới Item
            var newItem = new CartItem
            {
                CartID = cart.CartID,
                ProductID = productId,
                Quantity = quantity
            };
            _context.CartItems.Add(newItem);
        }

        await _context.SaveChangesAsync();
        return Ok("Đã cập nhật giỏ hàng thành công");
    }

    // 3. XÓA MÓN ĐỒ KHỎI GIỎ
    [HttpDelete("remove-item/{cartItemId}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var item = await _context.CartItems.FindAsync(cartItemId);
        if (item == null) return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return Ok("Đã xóa sản phẩm khỏi giỏ");
    }
}