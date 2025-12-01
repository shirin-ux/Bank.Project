using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;

namespace LoanService.Infrastructure.Repositories.Investment
{
    public class InvestmentPlanReadRepository(TransactionDBUtility transactionDBUtility) : IInvestmentPlanReadRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;

        public async Task<bool> DeletAsync(InvestmentPlans plan, CancellationToken ct)
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

        public async Task<IEnumerable<InvestmentPlans>> GetActivePlansAsync(CancellationToken ct)
        {
            const string sql = @"SELECT * FROM InvestmentPlans where IsActive=1 And IsDelete=0 ORDER BY Id DESC;";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryAsync<InvestmentPlans>(sql);
        }

        public async Task<InvestmentPlans> GetByCodeAsync(string Code, CancellationToken ct)
        {
            const string sql = @"SELECT * FROM dbo.InvestmentPlans WHERE Id = @Id AND IsDeleted = 0;";
            await using var conn = _transactionDBUtility.GetSqlConnection();

            return await conn.QueryFirstOrDefaultAsync<InvestmentPlans>(
                new CommandDefinition(sql, new { Code = Code }, cancellationToken: ct));
        }

        public async Task<InvestmentPlans> GetByIdAsync(Guid Id, CancellationToken ct)
        {
            const string sql = @"SELECT * FROM dbo.InvestmentPlans WHERE Id = @Id AND IsDeleted = 0;";
            await using var conn = _transactionDBUtility.GetSqlConnection();

            return await conn.QueryFirstOrDefaultAsync<InvestmentPlans>(
                new CommandDefinition(sql, new { Id = Id }, cancellationToken: ct));
        }

        public async Task<InvestmentPlans?> GetPlanWithMetaAsync(InvestmentPlanType planType, CancellationToken ct)
        {

            const string sql = @"SELECT TOP (1) p.*FROM InvestmentPlans p WHERE p.PlanType = @PlanType ;

                                SELECT f.*
                                FROM InvestmentPlanFeatures f
                                INNER JOIN InvestmentPlans p ON p.Id = f.PlanId
                                WHERE p.PlanType = @PlanType
                                  
                                ORDER BY f.[Order];
                                SELECT q.*
                                FROM InvestmentPlanFaqs q
                                INNER JOIN InvestmentPlans p ON p.Id = q.PlanId
                                WHERE p.PlanType = @PlanType
                           
                                ORDER BY q.[Order];
                                ";

            var cmd = new CommandDefinition(
                sql,
                new { PlanType = planType },
                cancellationToken: ct);
            await using var conn = _transactionDBUtility.GetSqlConnection();
            using var grid = await conn.QueryMultipleAsync(cmd);

            var plan = grid.Read<InvestmentPlans>().SingleOrDefault();
            if (plan is null)
                return null;

            var features = grid.Read<InvestmentPlanFeature>().ToList();
            var faqs = grid.Read<InvestmentPlanFaq>().ToList();

            foreach (var feature in features)
                plan.AddFeature(feature);

            foreach (var faq in faqs)
                plan.AddFaq(faq);

            return plan;
        }



        public async Task<Guid> InsertAsync(InvestmentPlans plan, CancellationToken ct)
        {

            const string sql = @"INSERT INTO dbo.InvestmentPlans(Id, Code, Name, PlanType, MinAmount, ShortDescription, IsActive, IsDeleted, CreatedAtUtc, UpdatedAtUtc)
                                 VALUES
                                 (@Id, @Code, @Name, @PlanType, @MinAmount, @ShortDescription, @IsActive, @IsDeleted, @CreatedAtUtc, @UpdatedAtUtc);";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.ExecuteAsync(
                new CommandDefinition(sql, plan, cancellationToken: ct));

            return plan.Id;
        }

        public async Task<bool> UpdateAsync(InvestmentPlans plan, CancellationToken ct)
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
