using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        private readonly IConfiguration configuration;
        private readonly RoleManager<IdentityRole> roleManager;
        public IMapper _mapper;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.roleManager = roleManager;

            _mapper = mapper;

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

                // --- THÊM DÒNG NÀY ĐỂ TOKEN CHỨA USER ID ---
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id),

                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                authClaim.Add(new System.Security.Claims.Claim(ClaimTypes.Role, role));

                var identityRole = await roleManager.FindByNameAsync(role);
                // THÊM DÒNG IF NÀY:
                if (identityRole != null)
                {
                    var roleClaims = await roleManager.GetClaimsAsync(identityRole);
                    foreach (var claim in roleClaims)
                    {
                        if (claim.Type == "Permission")
                            authClaim.Add(new Claim("Permission", claim.Value));
                    }
                }
            }

            var authSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? "mac_dinh_neu_bi_null_123456"));

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
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,   // gán từ model
                LastName = model.LastName      // gán từ model
            };

            // Tạo user với password
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Kiểm tra và tạo role "User" nếu chưa tồn tại
                if (!await roleManager.RoleExistsAsync(AppRole.User))
                {
                    await roleManager.CreateAsync(new IdentityRole(AppRole.User));
                }
                // Gán role "User" cho user mới tạo
                await userManager.AddToRoleAsync(user, AppRole.User);
            }

            return result;
        }
    }
}
