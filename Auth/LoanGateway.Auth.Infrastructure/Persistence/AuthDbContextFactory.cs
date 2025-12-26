using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
namespace LoanGateway.Auth.Infrastructure.Persistence
{
    public class AuthDbContextFactory
        : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
      .SetBasePath(Path.Combine(
          Directory.GetCurrentDirectory(),
          "..",
          "LoanGateway.Auth.Api"
      ))
      .AddJsonFile("appsettings.json", optional: false)
      .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("AuthConnection")
            );

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}

