using LoanService.Application.Mapping;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;


namespace LoanService.Infrastructure.Configurations;

public static class MapsterConfiguration
{
    public static void RegisterMapster(this IServiceCollection services)
    {
        // 🔹 تنظیمات global Mapster
        var config = TypeAdapterConfig.GlobalSettings;

        // 🔹 ثبت mapping هایی که خودت ساختی
        BankMellatMappingConfig.RegisterMappings();

        // 🔹 ثبت سرویس‌ها در DI
        services.AddSingleton(config);
        services.AddScoped<IMapper, Mapper>();
    }
}
