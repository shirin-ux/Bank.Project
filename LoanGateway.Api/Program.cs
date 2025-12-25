using Bank.Mellat.Infrastructure.Services;
using Bank.Mellat.Provider;
using Common;
using FluentValidation;
using Hangfire;
using Karizmah.Provider;
using LoanGateway.Infrastructure.Utility;
using LoanService.Api;
using LoanService.Api.Middlewares;
using LoanService.Application.Contracts;
using LoanService.Application.Mapping;
using LoanService.Application.PloicyProvider;
using LoanService.Application.PloicyProvider.Mellat;
using LoanService.Application.UseCase;
using LoanService.Application.UseCase.Command.StartLoanRequestDto;
using LoanService.Domain.IRepository.Investment;
using LoanService.Domain.IRepository.Loan;
using LoanService.Infrastructure;
using LoanService.Infrastructure.Configurations;
using LoanService.Infrastructure.Contracts;
using LoanService.Infrastructure.Jobs;
using LoanService.Infrastructure.Repositories.Investment;
using LoanService.Infrastructure.Repositories.Loan;
using LoanService.Infrastructure.Services;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SadadProvider;
using Shahkar.Provider;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("TransactionDB");


builder.Services.AddHttpClient("MellatApi", client =>
{
    client.BaseAddress = new Uri("https://gw4t.chub.behsazan.com");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = new CookieContainer()
});

builder.Services.Configure<KarizmahInvestmentOptions>(
    builder.Configuration.GetSection("Karizmah"));

builder.Services.AddHttpClient("KarizmahApi", (sp, client) =>
{
    var opt = sp.GetRequiredService<IOptions<KarizmahInvestmentOptions>>().Value;
    client.BaseAddress = new Uri(opt.BaseUrlApi);

});
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LoanService.Application.AssemblyReference).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(LoanService.Application.AssemblyReference).Assembly);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<ILoanNotificationBus, RabbitMqLoanNotificationBus>();
var root = builder.Environment.ContentRootPath;
var rulesPath = Path.Combine(root, builder.Configuration["MellatPolicy:RulesPath"]);
builder.Services.AddMemoryCache();
builder.Services.Configure<MellatPolicyOptions>(options =>
{
    options.RulesPath = rulesPath;
});


builder.Services.Configure<MellatApiOptions>(
    builder.Configuration.GetSection("MellatApiOptions"));

builder.Services.AddHttpClient();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();

//builder.Services.AddAuthentication("Bearer")
//.AddJwtBearer(options =>
//{
//    //options.Authority = "https://auth.yourdomain.com";
//    options.Authority = "https://192.168.87.12:3000";
//    options.TokenValidationParameters = new()
//    {
//        ValidateAudience = true,
//        ValidAudience = "internal-services"
//    };
//});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InternalServicePolicy", policy =>
    {
        policy.AddAuthenticationSchemes("Service");
        policy.RequireClaim("client_id", "investment-service");
        policy.RequireClaim("scope", "auth.internal");
    });
});

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMqOptions"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt")
);
var jwtOptions = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtOptions>();
builder.Services.AddSingleton<ILoanNotificationBus, RabbitMqLoanNotificationBus>();
builder.Services.AddSingleton<TransactionDBUtility>();
builder.Services.AddScoped<MellatBankProvider>();

var config = TypeAdapterConfig.GlobalSettings;
builder.Services.AddSingleton(config);


builder.Services.RegisterMapster();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var swaggerSettings = new SwaggerSettings();
builder.Configuration.GetSection("Swagger").Bind(swaggerSettings);

builder.Services.AddSingleton(swaggerSettings);

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


builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddHttpClient<MellatBankService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://gw4t.chub.behsazan.com/api/fs-contract-management");
        c.Timeout = TimeSpan.FromSeconds(60);
    });
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "LoanGateway.Auth.Api",
            ValidAudience = "LoanService.Api",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<LoanRequestOrchestrator>();
builder.Services.AddHttpContextAccessor();

BankMellatMappingConfig.RegisterMappings();
builder.Services.AddScoped<IBankPolicyFactory, BankPolicyFactory>();
builder.Services.AddScoped<IMellatBankService, MellatBankService>();
builder.Services.AddScoped<IProviderBase, MellatBankProvider>();
//builder.Services.AddScoped<IProviderBase, SamanBankProvider>();
builder.Services.AddScoped<IProviderBase, KarizmahInvestmentProvider>();
builder.Services.AddScoped<IInquiryInfoRepository, InquiryInfoRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IInstallmentRepository, InstallmentRepository>();
builder.Services.AddScoped<IPayResponseInfoRepository, PayResponseInfoRepository>();
builder.Services.AddScoped<IContractFileStorage, FileSystemContractFileStorage>();
builder.Services.AddScoped<ISadadService, SadadService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IGiftCardRepository, GiftCardRepository>();

builder.Services.AddScoped<IProviderFactory, ProviderFactory>();

// ?? Hangfire
builder.Services.AddHangfire(config =>
{
    config
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"));

});


builder.Services.AddHangfireServer();


// ?? Loan services

builder.Services.AddScoped<LoanJobRunner>();

builder.Services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();
builder.Services.AddScoped<IInvestmentProvider, KarizmahInvestmentProvider>();
builder.Services.AddScoped<IKarizmahService, KarizmahService>();
builder.Services.AddScoped<IInvestmentPlanReadRepository, InvestmentPlanReadRepository>();
builder.Services.AddScoped(typeof(IBankPolicy<>), typeof(MellatPolicy<>));
builder.Services.AddScoped<ILoanOrchestratorJobRunner, LoanOrchestratorJobs>();
builder.Services.AddScoped<IInvestmenJobRunner, KarizmahDailyIndexSyncJob>();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddScoped<IUserReadService, UserReadService>();
builder.Services.AddScoped<IShahkarService, ShahkarService>();

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
//builder.Services
//    .AddReverseProxy()
//    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();
//app.MapReverseProxy();
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IInvestmenJobRunner>(
        "karizmah-index-history-warmup",
        job => job.ExecuteAsync(CancellationToken.None),
        "15 20 * * *");
}



if (swaggerSettings.Enabled)
{
    app.UseSwagger(c =>
    {

        c.RouteTemplate = $"{swaggerSettings.RoutePrefix}/{{documentName}}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(
            $"/{swaggerSettings.RoutePrefix}/{swaggerSettings.Version}/swagger.json",
            $"{swaggerSettings.Title} {swaggerSettings.Version}");


        c.RoutePrefix = swaggerSettings.RoutePrefix;
    });
}

// ?? Middleware

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");


app.UseCors("Frontend");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();
app.Run();