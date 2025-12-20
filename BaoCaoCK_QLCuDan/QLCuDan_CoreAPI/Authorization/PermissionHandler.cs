using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models;
using System.Linq;
using System.Security.Claims;

namespace QLCuDan_CoreAPI.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly QuanLyChungCuDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionHandler(
            QuanLyChungCuDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Lấy MaCuDan từ token (claim)
            var maCuDanClaim = context.User.FindFirst("MaCuDan")?.Value;
            if (string.IsNullOrEmpty(maCuDanClaim))
            {
                return; // Không có MaCuDan trong token
            }

            if (!int.TryParse(maCuDanClaim, out int maCuDan))
            {
                return; // MaCuDan không hợp lệ
            }

            // Tìm user trong DB theo MaCuDan
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.MaCuDan == maCuDan);

            if (user == null)
            {
                return; // Không tìm thấy user
            }

            // Lấy tất cả roles của user
            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles == null || !userRoles.Any())
            {
                return; // User không có role nào
            }

            // Kiểm tra permission trong các roles
            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    // Lấy tất cả claims của role từ DB
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    
                    // Kiểm tra xem role có permission cần thiết không
                    var hasPermission = roleClaims
                        .Any(claim => claim.Type == "Permission" 
                                   && claim.Value.Equals(requirement.Permission, StringComparison.OrdinalIgnoreCase));

                    if (hasPermission)
                    {
                        context.Succeed(requirement);
                        return; // Đã tìm thấy permission, không cần check tiếp
                    }
                }
            }
        }
    }
}

