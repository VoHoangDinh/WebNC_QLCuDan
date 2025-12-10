using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QLCuDan_CoreAPI.DTO;
using QLCuDan_CoreAPI.Mapper;
using QLCuDan_CoreAPI.Models;
using QLCuDan_CoreAPI.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace QLCuDan_CoreAPI.Service
{
    public class AccountService : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private readonly QuanLyChungCuDbContext _context;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            QuanLyChungCuDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            _context = context;
        }

        public async Task<string> SignInAsync(SignInModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return "Invalid Authentication";
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                return "Invalid Authentication";
            }


            var authClaim = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.Name,model.Email),
                new System.Security.Claims.Claim(ClaimTypes.Email,model.Email),
                new Claim("MaCuDan", user.MaCuDan?.ToString() ?? ""), // ✅ Cư dân
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                authClaim.Add(new System.Security.Claims.Claim(ClaimTypes.Role, role));

                var identityRole = await roleManager.FindByNameAsync(role);
                var roleClaims = await roleManager.GetClaimsAsync(identityRole);

                // THÊM PERMISSION VÀO TOKEN
                foreach (var claim in roleClaims)
                {
                    if (claim.Type == "Permission")
                        authClaim.Add(new Claim("Permission", claim.Value));
                }
            }

            var authSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaim,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IdentityResult> SignUpAsync(SignUpModel model)
        {
            // 1️⃣ Kiểm tra Email có trong bảng CuDan chưa
            var cuDan = await _context.CuDans
                .SingleOrDefaultAsync(x => x.Email == model.Email);

            if (cuDan == null)
            {
                return IdentityResult.Failed(
                    new IdentityError { Description = "Email không tồn tại trong danh sách cư dân" }
                );
            }

            // 2️⃣ Kiểm tra email đã có tài khoản chưa
            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return IdentityResult.Failed(
                    new IdentityError { Description = "Email này đã được đăng ký tài khoản" }
                );
            }

            // 3️⃣ Tạo user và LIÊN KẾT cư dân ✅
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                MaCuDan = cuDan.MaCuDan // ✅ LIÊN KẾT
            };

            var result = await userManager.CreateAsync(user, model.Password);

            // 4️⃣ Gán role nếu thành công
            if (result.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync(AppRole.User))
                {
                    await roleManager.CreateAsync(new IdentityRole(AppRole.User));
                }

                await userManager.AddToRoleAsync(user, AppRole.User);
            }

            return result;
        }

    }
}