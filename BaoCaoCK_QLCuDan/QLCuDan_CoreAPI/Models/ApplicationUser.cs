using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_CoreAPI.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        // ✅ FK tới Cư Dân
        public int? MaCuDan { get; set; }

        // ✅ Navigation (khuyên dùng)
        [ForeignKey("MaCuDan")]
        public virtual CuDan? CuDan { get; set; }
    }
}
