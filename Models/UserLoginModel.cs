namespace YemekTarifleri.Models
{
    public class UserLoginModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Email { get; set; } =string.Empty;

        public string PasswordControl { get; set; }=string.Empty;
        //public string UserRole { get; set; }

    }
}
