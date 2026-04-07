using Microsoft.AspNetCore.Identity;

namespace mobileshopping.Models
{
    // Kế thừa IdentityUser để có sẵn các trường Id, Email, PasswordHash...
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string AddressCompany { get; set; }
        public string AddressHome { get; set; }
        public string AvatarURL { get; set; }
        public int UserID { get; internal set; }
    }
}