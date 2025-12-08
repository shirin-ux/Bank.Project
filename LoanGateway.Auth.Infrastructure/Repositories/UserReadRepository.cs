using Dapper;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using System.Numerics;

namespace LoanGateway.Auth.Infrastructure.Repositories
{
    public sealed class UserReadRepository : IUserReadRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility;

        public UserReadRepository(TransactionDBUtility transactionDBUtility)
        {
            _transactionDBUtility = transactionDBUtility;
        }

        public async Task<bool> ExistsByMobileOrNationalCodeAsync(string mobileNumber, string nationalCode, CancellationToken ct)
        {
            const string sql = @"SELECT TOP (1) 1 FROM [User] WHERE IsDeleted = 0 AND (MobileNumber = @MobileNumber OR NationalCode = @NationalCode);";

            await using var connection = _transactionDBUtility.GetSqlConnection();

            var result = await connection.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { MobileNumber = mobileNumber, NationalCode = nationalCode }, cancellationToken: ct));

            return result.HasValue;
        }
    }
}
