using Common;
using LoanGateway.Auth.Api;
using LoanGateway.Auth.Api.Middlewares;
using LoanGateway.Auth.Application;
using LoanGateway.Auth.Application.Adapter.User;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Communication;
using LoanGateway.Auth.Infrastructure.Persistence;
using LoanGateway.Auth.Infrastructure.Repositories;
using LoanGateway.Auth.Infrastructure.Services;
using LoanGateway.Auth.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shahkar.Provider;
using Sms.Provider;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection("RateLimit"));
builder.Services.AddScoped<RateLimitOptions>(sp =>
    sp.GetRequiredService<IOptions<RateLimitOptions>>().Value);

builder.Services.Configure<AuthCookieOptions>(
    builder.Configuration.GetSection("AuthCookie"));


var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];
var jwtSigningKey = jwtSection["SigningKey"];
var conn = builder.Configuration.GetConnectionString("TransactionDB");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role,

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSigningKey!)
            ),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
var swaggerSettings = new SwaggerSettings();
builder.Configuration.GetSection("Swagger").Bind(swaggerSettings);


builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc(swaggerSettings.Version, new OpenApiInfo
    {
        Title = swaggerSettings.Title,
        Version = swaggerSettings.Version
    });




    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = " Auth JWT  Bearer ",

        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddScoped<ISmsSender, SmsSender>();
builder.Services.AddScoped<IShahkarRepository, ShahkarRepository>();
builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IGiftCardEligibleUserRepository, GiftCardEligibleUserRepository>();
builder.Services.AddScoped<IUserOtpRepository, UserOtpRepository>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IShahkarProvider, ShahkarProvider>();
builder.Services.AddScoped<IShahkarService, ShahkarService>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthCookieService, AuthCookieService>();
builder.Services.AddScoped<IUserInfo, UserInfo>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();





builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AuthConnection")));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly));
builder.Services.Configure<KavenegarOptions>(builder.Configuration.GetSection("Kavenegar"));
builder.Services.Configure<UidApiOptions>(builder.Configuration.GetSection("UidApi"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
var app = builder.Build();
app.UseMiddleware<RateLimitMiddleware>();

if (swaggerSettings.Enabled)
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = $"{swaggerSettings.RoutePrefix}/{{documentName}}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = swaggerSettings.RoutePrefix;


        c.SwaggerEndpoint($"{swaggerSettings.Version}/swagger.json",
            $"{swaggerSettings.Title} {swaggerSettings.Version}");
    });
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();


app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();

app.Run();
