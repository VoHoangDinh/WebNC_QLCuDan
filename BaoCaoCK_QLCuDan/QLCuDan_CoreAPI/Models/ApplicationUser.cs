using Microsoft.AspNetCore.Identity;

namespace QLCuDan_CoreAPI.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
