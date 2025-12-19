namespace QLCuDan_CoreAPI.DTO
{
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}
