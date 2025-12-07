using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QLCuDan_CoreAPI.Models; // <--- Sửa dòng này thành namespace chứa Models của bạn
using QLCuDan_CoreAPI.Repository;
using QLCuDan_CoreAPI.Service;
using System.Text;
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
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập token JWT."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});




//cấu hình security jwt
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<QuanLyChungCuDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(
    option =>
    {
        option.SaveToken = true;
        option.RequireHttpsMetadata = false;
        option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                                   .GetBytes(builder.Configuration["JWT:Secret"]))


        };

    }
    );

//thêm auto mapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IAccountRepository, AccountService>();

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
// them cái này
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();