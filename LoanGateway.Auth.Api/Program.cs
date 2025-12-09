using LoanGateway.Auth.Api.Middlewares;
using LoanGateway.Auth.Application;
using LoanGateway.Auth.Application.Common;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Communication;
using LoanGateway.Auth.Infrastructure.Persistence;
using LoanGateway.Auth.Infrastructure.Repositories;
using Shahkar.Provider;
using Sms.Provider;


var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("TransactionDB");

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ISmsSender,SmsSender>();
builder.Services.AddScoped<IShahkarRepository, ShahkarRepository>();
builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
builder.Services.AddScoped<IUserOtpRepository, UserOtpRepository>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IShahkarProvider, ShahkarProvider>();
builder.Services.AddScoped<IShahkarService, ShahkarService>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();            

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<TransactionDBUtility>();
builder.Services.AddMediatR(cfg =>cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly));
builder.Services.Configure<KavenegarOptions>(builder.Configuration.GetSection("Kavenegar"));
builder.Services.Configure<UidApiOptions>(builder.Configuration.GetSection("UidApi"));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAll");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();

app.Run();
