using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.IRepository.Investment;

namespace LoanService.Infrastructure.Repositories.Investment
{
    public class InvestmentPlanReadRepository(TransactionDBUtility transactionDBUtility) : IInvestmentPlanReadRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;
        public async Task<IEnumerable<InvestmentPlan>> GetActivePlansAsync(CancellationToken ct)
        {
            const string sql = "SELECT * FROM InvestmentPlan ORDER BY IsActive DESC";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryAsync<InvestmentPlan>(sql);
        }
    }
}
