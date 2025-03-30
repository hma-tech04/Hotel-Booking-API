using System.Text;
using System.Text.Json.Serialization;
using API.Data;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Config AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Config DbContext
builder.Services.AddDbContext<HotelBookingContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Config JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings") ?? throw new Exception("JwtSettings is missing.");
var secretKey = jwtSettings["Secret"] ?? throw new Exception("JWT Secret is missing.");
var issuer = jwtSettings["Issuer"] ?? throw new Exception("JWT Issuer is missing.");
var audience = jwtSettings["Audience"] ?? throw new Exception("JWT Audience is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
        ?? throw new Exception("Google ClientId is missing.");
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
        ?? throw new Exception("Google ClientSecret is missing.");
    options.CallbackPath = builder.Configuration["Authentication:Google:CallbackPath"]
        ?? throw new Exception("Google CallbackPath is missing.");
});

// Config Authorization
builder.Services.AddAuthorization();
IdentityModelEventSource.ShowPII = true;

// Config Redis
var redisConnectionString = builder.Configuration["Redis:ConnectionString"]
    ?? throw new Exception("Redis connection string is missing.");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

// Config Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserAdminService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<GoogleService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<BookingService>();

// Config Controller
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Tắt xử lý validation mặc định của ASP.NET Core
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = false;
});

// Config Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);


// Config HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware xử lý exception
app.UseMiddleware<ExceptionMiddleware>();

// Middleware xác thực & ủy quyền
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Config endpoints
app.MapControllers();

app.Run();
