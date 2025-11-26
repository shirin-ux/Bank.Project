using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.IRepository.Investment;

namespace LoanService.Infrastructure.Repositories.Investment
{
    public class InvestmentPlanReadRepository(TransactionDBUtility transactionDBUtility) : IInvestmentPlanReadRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;

        public async Task<bool> DeletAsync(InvestmentPlan plan, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            const string sql = @"
                                UPDATE dbo.InvestmentPlans
                                SET IsDeleted   = 1,IsActive    = 0, UpdatedAtUtc = @Now
                                WHERE Id = @Id AND IsDeleted = 0;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            var affected = await conn.ExecuteAsync(
                new CommandDefinition(sql, new { Id = plan.Id, Now = now }, cancellationToken: ct));

            return affected > 0;
        }

        public async Task<IEnumerable<InvestmentPlan>> GetActivePlansAsync(CancellationToken ct)
        {
            const string sql = @"SELECT * FROM InvestmentPlan where IsActive=1 And IsDelete=0 ORDER BY Id DESC;";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryAsync<InvestmentPlan>(sql);
        }

        public async Task<InvestmentPlan> GetByCodeAsync(string Code, CancellationToken ct)
        {
            const string sql = @"SELECT * FROM dbo.InvestmentPlans WHERE Id = @Id AND IsDeleted = 0;";
            await using var conn = _transactionDBUtility.GetSqlConnection();

            return await conn.QueryFirstOrDefaultAsync<InvestmentPlan>(
                new CommandDefinition(sql, new { Code=Code }, cancellationToken: ct));
        }

        public async Task<InvestmentPlan> GetByIdAsync(Guid Id, CancellationToken ct)
        {
            const string sql = @"SELECT * FROM dbo.InvestmentPlans WHERE Id = @Id AND IsDeleted = 0;";
            await using var conn = _transactionDBUtility.GetSqlConnection();

            return await conn.QueryFirstOrDefaultAsync<InvestmentPlan>(
                new CommandDefinition(sql, new { Id = Id }, cancellationToken: ct));
        }

        public async Task<Guid> InsertAsync(InvestmentPlan plan, CancellationToken ct)
        {

            const string sql = @"INSERT INTO dbo.InvestmentPlans(Id, Code, Name, PlanType, MinAmount, ShortDescription, IsActive, IsDeleted, CreatedAtUtc, UpdatedAtUtc)
                                 VALUES
                                 (@Id, @Code, @Name, @PlanType, @MinAmount, @ShortDescription, @IsActive, @IsDeleted, @CreatedAtUtc, @UpdatedAtUtc);";
            
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.ExecuteAsync(
                new CommandDefinition(sql, plan, cancellationToken: ct));

            return plan.Id;
        }

        public async Task<bool> UpdateAsync(InvestmentPlan plan, CancellationToken ct)
        {
            const string sql = @"UPDATE dbo.InvestmentPlans
                                 SET Code             = @Code,
                                     Name             = @Name,
                                     PlanType         = @PlanType,
                                     MinAmount        = @MinAmount,
                                     ShortDescription = @ShortDescription,
                                     IsActive         = @IsActive,
                                     UpdatedAtUtc     = @UpdatedAtUtc
                                 WHERE Id = @Id AND IsDeleted = 0;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            var affected = await conn.ExecuteAsync(
                new CommandDefinition(sql, plan, cancellationToken: ct));

            return affected > 0;
        }
    }
}
