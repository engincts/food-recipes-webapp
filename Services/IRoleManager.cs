namespace YemekTarifleri.Services
{
    public interface IRoleManager
    {
        //public Task CreateRoleAsync(string roleName);
        public  Task AssignRoleToUserAsync(int userId, string roleName);
    }
}
