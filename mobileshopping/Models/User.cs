namespace mobileshopping.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string AddressCompany { get; set; }
        public string AddressHome { get; set; }
        public string AvatarURL { get; set; }
    }
}
