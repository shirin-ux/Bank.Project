using Bank.Mellat.Infrastructure.Services;
using Bank.Mellat.Provider;
using Common;
using Hangfire;
using LoanGateway.Infrastructure.Utility;
using LoanService.Application.Contracts;
using LoanService.Application.Mapping;
using LoanService.Application.PloicyProvider;
using LoanService.Application.PloicyProvider.Mellat;
using LoanService.Application.UseCase.Command.CustomerInquiry;
using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.GetCollateralContractFile;
using LoanService.Application.UseCase.Command.GetContractFile;
using LoanService.Application.UseCase.Command.GetCustomerBilling;
using LoanService.Application.UseCase.Command.GetCustomerCreditBalance;
using LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;
using LoanService.Application.UseCase.Command.OtpRequest;
using LoanService.Application.UseCase.Command.RepaymentReques;
using LoanService.Application.UseCase.Command.StartLoanRequestDto;
using LoanService.Application.UseCase.Command.SubmitPayRequest;
using LoanService.Application.UseCase.Command.TransferRegister;
using LoanService.Application.UseCase.Query.CustomerInquiryStatus;
using LoanService.Application.UseCase.Query.GetInstallments;
using LoanService.Application.UseCase.Query.PayResponse;
using LoanService.Application.UseCase.Query.ReturnTransferReport;
using LoanService.Application.UseCase.Query.TransferInquiry;
using LoanService.Domain.Entities;
using LoanService.Domain.IRepository;
using LoanService.Infrastructure.Configurations;
using LoanService.Infrastructure.Jobs;
using LoanService.Infrastructure.Repositories;
using LoanService.Infrastructure.Services;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

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
//builder.Services.Configure<MellatPolicyRulesOptions>(
//    builder.Configuration.GetSection("MellatPolicyRules"));

var root = builder.Environment.ContentRootPath;
var rulesPath = Path.Combine(root, builder.Configuration["MellatPolicy:RulesPath"]);

builder.Services.Configure<MellatPolicyOptions>(options =>
{
    options.RulesPath = rulesPath;
});


builder.Services.Configure<MellatApiOptions>(
    builder.Configuration.GetSection("MellatApiOptions"));




builder.Services.AddSingleton<TransactionDBUtility>();
builder.Services.AddScoped<MellatBankProvider>();

var config = TypeAdapterConfig.GlobalSettings;
builder.Services.AddSingleton(config);


builder.Services.RegisterMapster();


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


builder.Services.AddHttpClient<MellatBankService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://gw4t.chub.behsazan.com/api/fs-contract-management");
        c.Timeout = TimeSpan.FromSeconds(60);
    });
//.ConfigurePrimaryHttpMessageHandler(sp =>
//{

//    var opt = sp.GetRequiredService<IOptions<MellatApiOptions>>().Value;
//    return new HttpClientHandler
//    {
//        Proxy = new WebProxy(opt.Proxy),
//        UseProxy = true
//    };
//});
//builder.Services.AddMediatR(cfg =>
//{
//    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
//});
builder.Services.AddScoped<LoanRequestOrchestrator>();


BankMellatMappingConfig.RegisterMappings();
builder.Services.AddScoped<IBankPolicyFactory, BankPolicyFactory>();
builder.Services.AddScoped<IMellatBankService, MellatBankService>();
builder.Services.AddScoped<IBankProvider, MellatBankProvider>();
builder.Services.AddScoped<IInquiryInfoRepository, InquiryInfoRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IInstallmentRepository, InstallmentRepository>();
builder.Services.AddScoped<IPayResponseInfoRepository, PayResponseInfoRepository>();
builder.Services.AddScoped<IContractFileStorage, FileSystemContractFileStorage>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LoanService.Application.AssemblyReference).Assembly));
//builder.Services.AddMediatR(cfg =>
//{
//    cfg.RegisterServicesFromAssemblies(
//                typeof(CustomerInquiryHandler).Assembly,
//                typeof(DepositRequestHandler).Assembly,
//                typeof(GetCollateralContractFileHandler).Assembly,
//                typeof(GetContractFileHandler).Assembly,
//                typeof(GetCustomerBillingHandler).Assembly,
//                typeof(GetCustomerCreditBalanceHandler).Assembly,
//                typeof(GetCustomerPurchaseDetailsHandler).Assembly,
//                typeof(OtpRequestHandler).Assembly,
//                typeof(RepaymentRequestHandler).Assembly,
//                typeof(SubmitPayRequestHandler).Assembly,
//                typeof(TransferRegisterHandler).Assembly,
//                typeof(CustomerInquiryStatusHandler).Assembly,
//                typeof(GetInstallmentsHandler).Assembly,
//                typeof(GetPayResponseHandler).Assembly,
//                typeof(ReturnTransferReportHandler).Assembly,
//                typeof(TransferInquiryHandler).Assembly

//    );
//});
builder.Services.AddScoped<IBankProviderFactory, BankProviderFactory>();

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
builder.Services.AddScoped(typeof(IBankPolicy<>), typeof(MellatPolicy<>));
builder.Services.AddScoped<ILoanOrchestratorJobRunner, LoanOrchestratorJobs>();
builder.Services.AddCors(o => o.AddPolicy("AllowAll",
    p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();

// ?? Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");


app.UseCors("AllowAll");
app.MapControllers();
app.Run();