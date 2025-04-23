using System;
using System.Collections.Generic;

namespace YemekTarifleri.Db;

public partial class UserRole
{
    public int RoleId { get; set; }

    public int UserRoleId { get; set; }

    public int UserId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
