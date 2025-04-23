using YemekTarifleri.Models;

namespace YemekTarifleri.Services
{
    public interface IUserManager
    {
        public bool Login(Models.UserLoginModel user, out int userId);
        //public bool CheckLogin(UserLoginModel user, out string userRole);
        public bool CreateUser(Models.UserLoginModel user, out int userId);
        public bool DeleteUser(int userId);
        public bool UpdateUser(UserUpdateModel updateModel);



    }
}
