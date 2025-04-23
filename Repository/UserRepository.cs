using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using YemekTarifleri.Db;
using YemekTarifleri.Models;

namespace YemekTarifleri.Repository
{
    public class UserRepository : Services.IUserManager
    {
      //public bool CheckLogin(UserLoginModel user, out string userRole)
      //  {
      //      if (user.UserRole == "admin" && user.Password == "password")
      //      {
      //          userRole = "Admin";
      //          return true;
      //      }
      //      else if (user.UserRole == "moderator" && user.Password == "password")
      //      {
      //          userRole = "Moderator";
      //          return true;
      //      }
      //      else if (user.UserRole == "user" && user.Password == "password")
      //      {
      //          userRole = "User";
      //          return true;
      //      }
      //      else
      //      {
      //          userRole = null;
      //          return false;
      //      }
      //  }

        public UserLoginModel GetUserFromDatabase(string username)
        {
            // Veritabanından kullanıcıyı alın
            // Bu bir örnektir, veritabanı erişimi için uygun kodu ekleyin
            return new UserLoginModel
            {
                UserName = username,
                Password = "hashedPasswordFromDatabase" // Hashlenmiş şifre veritabanından alınmalıdır
            };
        }
        public string GetUserRole(string userName)
        {
            // Burada rolü veritabanından veya başka bir kaynaktan almanız gerekiyor
            // Bu bir örnektir, rol bilgilerini veritabanından alın
            if (userName == "admin")
            {
                return "Admin";
            }
            else if (userName == "moderator")
            {
                return "Moderator";
            }
            else
            {
                return "User";
            }
        }
        public string HashPassword(string password)
        {
            // Şifreyi hash'leyin
            var salt = new byte[128 / 8];
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
        public bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            string password2 = HashPassword(enteredPassword);

            return storedHashedPassword == password2;
        }

        public bool CreateUser(Models.UserLoginModel user, out int userId)
        {
            userId = 0; // Default olarak userId'yi sıfır olarak ayarla

            if (user == null) return false;

            using (YemekTarifleriContext dbcontext = new YemekTarifleriContext())
            {
                var existingUser = dbcontext.AppUsers.FirstOrDefault(u => u.UserName == user.UserName);
                var hashedPassword = HashPassword(user.Password);

                // Eğer kullanıcı zaten varsa veya şifreler uyuşmuyorsa false döndür
                if (existingUser != null || user.Password != user.PasswordControl)
                {
                    return false;
                }

                // Yeni kullanıcı oluştur
                var newAppUser = new AppUser
                {
                    UserName = user.UserName,
                    Password = hashedPassword,
                };

                dbcontext.AppUsers.Add(newAppUser);
                dbcontext.SaveChanges(); // Yeni kullanıcıyı kaydet

                // Yeni oluşturulan kullanıcının ID'sini al
                userId = newAppUser.UserId;

                // Yeni kullanıcıya varsayılan olarak "User" rolü atama
                Util.user.AssignRoleToUserAsync(newAppUser.UserId, "User");

                return true;
            }
        }

        public bool DeleteUser(int userId)
        {
            using (YemekTarifleriContext dbContext = new YemekTarifleriContext())
            {
                var userToDelete = dbContext.AppUsers.FirstOrDefault(u => u.UserId == userId);
                if (userToDelete == null)
                {
                    return false;
                }

                dbContext.AppUsers.Remove(userToDelete);
                dbContext.SaveChanges();
                return true;
            }
        }
        public async Task AssignRoleToUserAsync(int userId, string roleName)
        {
            using (YemekTarifleriContext _context = new YemekTarifleriContext())
            {
                var user = await _context.AppUsers.FindAsync(userId);
                if (user == null)
                    throw new Exception("User not found.");

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null)
                    throw new Exception("Role not found.");

                if (await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.RoleId))
                    throw new Exception("User already has this role.");

                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = role.RoleId
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }
        }
        public List<AppUser> GetAllUsers()
        {
            using (var dbContext = new YemekTarifleriContext())
            {
                return dbContext.AppUsers.ToList();
            }
        }



        public bool Login(UserLoginModel user, out int userId)
        {

            Db.YemekTarifleriContext dbContext = new Db.YemekTarifleriContext();

            var existingUser = dbContext.AppUsers
            .Where(u => u.UserName == user.UserName)
            .Select(u => new { u.UserId, u.UserName, u.Password }) // İlgili alanları seçin
            .FirstOrDefault();


            // Eğer kullanıcı mevcutsa ve şifre doğrulama başarılıysa giriş yap
            if (existingUser != null && VerifyPassword(user.Password, existingUser.Password)) //salt saklamak lazımmış
            {
                userId = existingUser.UserId;
                return true;
            }
            userId = 0;
            return false;
        }

        public bool UpdateUser(UserUpdateModel updateModel)
        {
            using(var _context=new YemekTarifleriContext())
            { 
                var existingUser = _context.AppUsers
                    .FirstOrDefault(u => u.UserName == updateModel.OldUserName && u.Password == HashPassword(updateModel.OldPassword));

                if (existingUser == null)
                {
                    return false; // User not found or password does not match
                }

                // Update user details
                existingUser.UserName = updateModel.NewUserName;
                existingUser.Password = HashPassword(updateModel.NewPassword);

                try
                {
                    _context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    // Log exception if necessary
                    return false;
                }

            }
        }
        public async Task<AppUser> GetUserByUserName(string userName)
        {
            using (var context = new YemekTarifleriContext())
            {
                return await context.AppUsers
                    .FirstOrDefaultAsync(u => u.UserName == userName);
            }
        }


    }
}
