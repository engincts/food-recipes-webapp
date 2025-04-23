using Microsoft.EntityFrameworkCore;
using YemekTarifleri.Db;
using YemekTarifleri.Services;

namespace YemekTarifleri.Repository
{
    public class RoleManager:IRoleManager
    {
        //public async Task CreateRoleAsync(string roleName)
        //{
        //    using (YemekTarifleriContext _context = new YemekTarifleriContext())
        //    {
        //        if (await _context.Roles.AnyAsync(r => r.RoleName == roleName))
        //            throw new Exception("Role already exists.");

        //        var role = new Role { RoleName = roleName };
        //        _context.Roles.Add(role);
        //        await _context.SaveChangesAsync();
        //    }
        //}

        public async Task AssignRoleToUserAsync(int userId, string roleName)
        {
            using(YemekTarifleriContext _context=new YemekTarifleriContext())
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
                    UserRoleId = role.RoleId,
                    RoleId = role.RoleId
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Role>> GetRolesByUserIdAsync(int userId)
        {
            using (YemekTarifleriContext _context = new YemekTarifleriContext())
            {
                return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();
            }
        }

        public async Task<List<AppUser>> GetUsersInRoleAsync(string role)
        {
            using (YemekTarifleriContext context = new YemekTarifleriContext())
            {
                // Rol adını kullanarak RoleId'yi buluyoruz
                var roleId = await context.Roles
                    .Where(r => r.RoleName == role)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                if (roleId == 0)
                {
                    return new List<AppUser>(); // Eğer rol bulunamazsa boş liste döndür
                }

                // RoleId'yi kullanarak kullanıcıları getiriyoruz
                return await context.UserRoles
                    .Where(ur => ur.RoleId == roleId)
                    .Select(ur => ur.User)
                    .ToListAsync();
            }
        }



    }
}
