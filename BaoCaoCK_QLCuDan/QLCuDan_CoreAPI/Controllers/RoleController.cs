using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QLCuDan_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
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
    }
}
