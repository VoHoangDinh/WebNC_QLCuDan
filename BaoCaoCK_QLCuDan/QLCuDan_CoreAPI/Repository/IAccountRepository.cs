using Microsoft.AspNetCore.Identity;
using QLCuDan_CoreAPI.DTO;

namespace QLCuDan_CoreAPI.Repository
{
    public interface IAccountRepository
    {
        Task<IdentityResult> SignUpAsync(SignUpModel model);
        Task<string> SignInAsync(SignInModel model);
    }
}
