using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using API.Data;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Config automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Config DbContext
builder.Services.AddDbContext<HotelBookingContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


// Config jwt
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
#pragma warning disable CS8604 // Possible null reference argument.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"])),
        ClockSkew = TimeSpan.Zero
    };

#pragma warning restore CS8604 // Possible null reference argument.
})
.AddGoogle(options =>
{
#pragma warning disable CS8601 // Possible null reference assignment.
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
#pragma warning restore CS8601 // Possible null reference assignment.
    options.CallbackPath = builder.Configuration["Authentication:Google:CallbackPath"];
});

builder.Services.AddAuthorization();
IdentityModelEventSource.ShowPII = true;

// Config redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis")["ConnectionString"];
#pragma warning disable CS8604 // Possible null reference argument.
    return ConnectionMultiplexer.Connect(configuration);
#pragma warning restore CS8604 // Possible null reference argument.
});

// Config Email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));


// add services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserAdminService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<GoogleService>();


// Config controller and filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

// Config endpoints
app.MapControllers();

app.Run();