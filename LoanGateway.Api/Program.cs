using Bank.Mellat.Infrastructure.Services;
using Bank.Mellat.Provider;
using Common;
using Hangfire;
using LoanGateway.Infrastructure.Utility;
using LoanService.Application.Contracts;
using LoanService.Application.Mapping;
using LoanService.Application.PloicyProvider.Mellat;
using LoanService.Application.PloicyProvider;
using LoanService.Domain.IRepository;
using LoanService.Infrastructure.Configurations;
using LoanService.Infrastructure.Jobs;
using LoanService.Infrastructure.Repositories;
using Mapster;
using Microsoft.OpenApi.Models;
using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("TransactionDB");
Console.WriteLine($"TransactionDB connection: {conn}");
// ?? Register TransactionDBUtility first
builder.Services.AddSingleton<TransactionDBUtility>();

// ?? Mapster config
var config = TypeAdapterConfig.GlobalSettings;
builder.Services.AddSingleton(config);

// ?? Mellat policy options
builder.Services.Configure<MellatPolicyOptions>(
    builder.Configuration.GetSection("MellatSettings")
);

// ?? Generic policy
//builder.Services.AddScoped(typeof(IMellatGenericPolicy<>), typeof(MellatGenericPolicy<>));

// ?? Register Mapster
builder.Services.RegisterMapster();

// ?? Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LoanService API",
        Version = "v1",
        Description = "API documentation for LoanService",
        Contact = new OpenApiContact
        {
            Name = "Your Team Name",
            Email = "support@yourcompany.com"
        }
    });
});

// ?? HttpClient + MediatR
builder.Services.AddHttpClient();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
});

// ?? Bank services
BankMellatMappingConfig.RegisterMappings();
builder.Services.AddScoped<IBankPolicyFactory, BankPolicyFactory>();
builder.Services.AddScoped<IMellatBankService, MellatBankService>();
builder.Services.AddScoped<IBankProvider, MellatBankProvider>();

// ?? Hangfire
builder.Services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(
              builder.Configuration.GetConnectionString("Hangfire"),
              new Hangfire.SqlServer.SqlServerStorageOptions
              {
                  SchemaName = "Hangfire",
                  QueuePollInterval = TimeSpan.FromSeconds(5)
              }));
builder.Services.AddHangfireServer();

// ?? Loan services
builder.Services.AddScoped<LoanJobRunner>();
builder.Services.AddScoped<ILoanRequestRepository, LoanRequestRepository>();
builder.Services.AddScoped(typeof(IBankPolicy<>), typeof(MellatPolicy<>));
builder.Services.AddScoped<ILoanJobs, HangfireLoanJobs>();

var app = builder.Build();

// ?? Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();