using Microsoft.EntityFrameworkCore;
using QLCuDan_CoreAPI.Models; // <--- Sửa dòng này thành namespace chứa Models của bạn
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// ==============================================================
// 1. CẤU HÌNH KẾT NỐI DATABASE (QUAN TRỌNG NHẤT)
// ==============================================================
// Lấy chuỗi kết nối từ file appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký DbContext vào hệ thống
builder.Services.AddDbContext<QuanLyChungCuDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==============================================================
// 2. CẤU HÌNH CORS (Để trang Web MVC gọi được API này)
// ==============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()   // Cho phép mọi nguồn gọi đến
                  .AllowAnyMethod()   // Cho phép mọi phương thức (GET, POST, PUT, DELETE)
                  .AllowAnyHeader();  // Cho phép mọi Header
        });
});

// Các dịch vụ mặc định khác
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==============================================================
// 3. CẤU HÌNH PIPELINE (Xử lý request)
// ==============================================================

// Bật Swagger để test API (chỉ hiện khi chạy Debug)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Kích hoạt CORS (Phải đặt trước UseAuthorization)
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();