using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLCuDan_CoreAPI.Models
{
    [Table("ToaNha")]
    public class ToaNha
    {
        [Key]
        public int MaToaNha { get; set; }
        public string TenToaNha { get; set; }
        public string? DiaChi { get; set; }
    }
}