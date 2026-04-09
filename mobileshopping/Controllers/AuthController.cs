using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mobileshopping.Models;
using System.Threading.Tasks;

namespace mobileshopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // 1. API ĐĂNG KÝ (Register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra xem Email đã tồn tại chưa
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                return BadRequest(new { message = "Email này đã được sử dụng!" });
            }

            // Tạo đối tượng User mới
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                // Gán giá trị rỗng thay vì để NULL
                AddressCompany = "",
                AddressHome = "",
                Gender = "Other",
                AvatarURL = "default.png",
                DateOfBirth = DateTime.Now
            };
            // UserManager sẽ tự động mã hóa mật khẩu và lưu vào Database
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = "Đăng ký tài khoản thành công!",
                    data = new { user.Id, user.Email, user.FullName }
                });
            }

            // Trả về danh sách lỗi nếu không thỏa mãn chính sách bảo mật (ví dụ mật khẩu quá ngắn)
            return BadRequest(new { success = false, errors = result.Errors });
        }

        // 2. API ĐĂNG NHẬP (Login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Tìm user theo Email trong Database
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Email không tồn tại trong hệ thống!" });
            }

            // Xác thực mật khẩu đã mã hóa bằng SignInManager
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                // Trả về thông tin user để App lưu lại phiên đăng nhập
                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công!",
                    data = new
                    {
                        user.Id,
                        user.Email,
                        user.FullName,
                        user.AvatarURL
                    }
                });
            }

            return Unauthorized(new { message = "Mật khẩu không chính xác!" });
        }
        // 3. API ĐĂNG XUẤT (Logout)
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie xác thực trên server (nếu có)
            await _signInManager.SignOutAsync();

            return Ok(new
            {
                success = true,
                message = "Đã đăng xuất thành công!"
            });
        }
    }

    // Các lớp Model bổ trợ cho Request
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }
}