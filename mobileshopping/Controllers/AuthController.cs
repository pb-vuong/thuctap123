using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mobileshopping.Models; 

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // API Đăng nhập
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Tìm user có Email và Password khớp với Database
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

        if (user == null)
        {
            return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
        }

        // Trả về thông tin user để App lưu lại phiên đăng nhập
        return Ok(new
        {
            success = true,
            message = "Đăng nhập thành công!",
            data = user
        });
    }
}

// Lớp phụ trợ để nhận dữ liệu từ giao diện
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}