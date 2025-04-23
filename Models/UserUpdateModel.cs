namespace YemekTarifleri.Models
{
    public class UserUpdateModel
    {
        public string? OldUserName { get; set; }
        public string? OldPassword { get; set; }
        public string? NewUserName { get; set; }
        public string? NewPassword { get; set; }
    }
}
