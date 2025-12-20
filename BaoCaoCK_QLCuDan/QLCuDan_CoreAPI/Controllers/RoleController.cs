using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QLCuDan_CoreAPI.Models;

namespace QLCuDan_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            var result = new List<object>();

            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

                result.Add(new
                {
                    Id = role.Id,
                    Name = role.Name,
                    Permissions = permissions
                });
            }

            return Ok(result);
        }

        [HttpGet("get-permissions")]
        public async Task<IActionResult> GetPermissions(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Tên role không được để trống");

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound("Role không tồn tại");

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

            return Ok(permissions);
        }

        [HttpPost("add-permission")]
        public async Task<IActionResult> AddPermission(string roleName, string permission)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return BadRequest("Role không tồn tại");

            await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));

            return Ok("Đã thêm quyền thành công");
        }

        [HttpPut("update-permission")]
        public async Task<IActionResult> UpdatePermission(string roleName, string oldPermission, string newPermission)
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(oldPermission) || string.IsNullOrWhiteSpace(newPermission))
                return BadRequest("Thông tin không được để trống");

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound("Role không tồn tại");

            var claims = await _roleManager.GetClaimsAsync(role);
            var oldClaim = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == oldPermission);

            if (oldClaim == null)
                return NotFound("Permission không tồn tại trong role này");

            // Kiểm tra permission mới đã tồn tại chưa
            var existingClaim = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == newPermission);
            if (existingClaim != null)
                return BadRequest("Permission mới đã tồn tại trong role này");

            // Xóa permission cũ và thêm permission mới
            var removeResult = await _roleManager.RemoveClaimAsync(role, oldClaim);
            if (!removeResult.Succeeded)
                return BadRequest($"Lỗi khi xóa permission cũ: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");

            var addResult = await _roleManager.AddClaimAsync(role, new Claim("Permission", newPermission));
            if (!addResult.Succeeded)
            {
                // Nếu thêm mới thất bại, khôi phục permission cũ
                await _roleManager.AddClaimAsync(role, oldClaim);
                return BadRequest($"Lỗi khi thêm permission mới: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
            }

            return Ok("Đã cập nhật permission thành công");
        }

        [HttpDelete("delete-permission")]
        public async Task<IActionResult> DeletePermission(string roleName, string permission)
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(permission))
                return BadRequest("Thông tin không được để trống");

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound("Role không tồn tại");

            var claims = await _roleManager.GetClaimsAsync(role);
            var claimToRemove = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permission);

            if (claimToRemove == null)
                return NotFound("Permission không tồn tại trong role này");

            var result = await _roleManager.RemoveClaimAsync(role, claimToRemove);

            if (result.Succeeded)
                return Ok("Đã xóa permission thành công");

            return BadRequest($"Lỗi khi xóa permission: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        [HttpPut("assign-role-to-user")]
        public async Task<IActionResult> AssignRoleToUser(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Thông tin không được để trống");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Người dùng không tồn tại");

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound("Role không tồn tại");

            // Xóa tất cả roles hiện tại của user
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return BadRequest($"Lỗi khi xóa role cũ: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
            }

            // Gán role mới
            var addResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
                return BadRequest($"Lỗi khi gán role: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");

            return Ok("Đã cập nhật vai trò cho người dùng thành công");
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                });
            }

            return Ok(result);
        }
    }
}
