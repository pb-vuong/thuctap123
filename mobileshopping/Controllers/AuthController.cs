using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using mobileshopping.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace mobileshopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration; // Thêm Configuration

        public AuthController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration configuration) // Inject IConfiguration
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        // 1. API ĐĂNG KÝ (Register) - Giữ nguyên logic cũ
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                return BadRequest(new { message = "Email này đã được sử dụng!" });
            }

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                AddressCompany = "",
                AddressHome = "",
                Gender = "Other",
                AvatarURL = "default.png",
                DateOfBirth = DateTime.Now
            };

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

            return BadRequest(new { success = false, errors = result.Errors });
        }

        // 2. API ĐĂNG NHẬP (Login) - CẬP NHẬT TRẢ VỀ TOKEN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Email không tồn tại trong hệ thống!" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                // SINH TOKEN TẠI ĐÂY
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công!",
                    token = token, // Trả token về cho client
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

        // PHƯƠNG THỨC SINH TOKEN (PRIVATE)
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            // Các thông tin đính kèm trong Token (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // 3. API ĐĂNG XUẤT (Logout)
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { success = true, message = "Đã đăng xuất thành công!" });
        }
    }

    public class LoginRequest { public string Email { get; set; } public string Password { get; set; } }
    public class RegisterRequest { public string Email { get; set; } public string Password { get; set; } public string FullName { get; set; } }
}