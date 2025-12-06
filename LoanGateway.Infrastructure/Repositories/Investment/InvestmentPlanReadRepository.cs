using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using System.Data;

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
            const string sql = @"SELECT * FROM InvestmentPlans where IsActive=1 And IsDelete=0  ORDER BY Id DESC;";
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

        public async Task UpsertDailyHistoryAsync(InvestmentPlanType plan, IReadOnlyList<InvestmentIndexHistory> points, CancellationToken ct)
        {
            try
            {
                if (points == null || points.Count == 0)
                    return;


                var daily = points.GroupBy(p => p.IndexDateTimeUtc).Select(g => new
                {
                    IndexDateTimeUtc = g.Key,
                    IndexValue = g.OrderBy(x => x.IndexDateTimeUtc).Last().IndexValue
                }).ToList();

                if (!daily.Any())
                    return;

                var tvp = new DataTable();
                tvp.Columns.Add("IndexDateTimeUtc", typeof(DateTime));
                tvp.Columns.Add("IndexValue", typeof(decimal));
                foreach (var d in daily)
                {
                    tvp.Rows.Add(d.IndexDateTimeUtc, d.IndexValue);
                }
                await using var conn = _transactionDBUtility.GetSqlConnection();
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync(ct);
                var param = new DynamicParameters();
                param.Add("@PlanType", (byte)plan, DbType.Byte);
                param.Add("@Points", tvp.AsTableValuedParameter("dbo.InvestmentIndexHistoryPointType"));

                await conn.ExecuteAsync(new CommandDefinition(
                    commandText: "InvestmentIndexHistory_upsetrRang",
                    parameters: param,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct

                    ));
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<IReadOnlyList<InvestmentIndexHistory>> GetRangeAsync(InvestmentPlanType plan, DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct)
        {
            using var conn = _transactionDBUtility.GetSqlConnection();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();
            var param = new
            {
                PlanType = (byte)plan,
                FromDateUtc = fromDateUtc,
                ToDateUtc = toDateUtc
            };
            var rows = await conn.QueryAsync(
                new CommandDefinition(
                    commandText: "dbo.InvestmentIndexHistory_GetRange",
                    commandType: CommandType.StoredProcedure,
                    parameters: param,
                    cancellationToken: ct

                    ));
            var result = rows.Select(r => new InvestmentIndexHistory
            {

                IndexDateTimeUtc = r.IndexDateTimeUtc,
                IndexValue = r.IndexValue
            }).ToList();

            return result;


        }

        public Task<InvestmentAccount?> GetByNationalCodeAndPlanAsync(string nationalCode, InvestmentPlanType planType, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(InvestmentAccount account, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<DateTime?> GetLastDateAsync(InvestmentPlanType plan, CancellationToken ct)
        {
            using var conn = _transactionDBUtility.GetSqlConnection();
            var param = new { PlanType = (byte)plan };

            var result = await conn.QuerySingleOrDefaultAsync<DateTime?>(
                "dbo.InvestmentIndexHistory_GetLastDate",
                param,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task DeleteOlderThanAsync(InvestmentPlanType plan, DateTime cutoffUtc, CancellationToken ct)
        {
            using var conn = _transactionDBUtility.GetSqlConnection();
            var param = new { PlanType = (byte)plan, CutoffDateUtc = cutoffUtc.Date };

            await conn.ExecuteAsync(
                "dbo.InvestmentIndexHistory_DeleteOlderThan",
                param,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<InvestmentPlans> GetPlanAsync(string title, CancellationToken ct)
        {
            const string sql = @"SELECT * FROM InvestmentPlans where IsActive=1 And IsDelete=0 AND Title=@title ORDER BY Id DESC;";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryFirstOrDefaultAsync<InvestmentPlans>(new CommandDefinition(sql, new { Title = title }, cancellationToken: ct));
        }

        public async Task<List<InvestmentIndexHistory>> GetLatestPointsAsync(InvestmentPlanType planType, int count, CancellationToken ct)
        {
            const string sql = @"SELECT TOP (@Count) IndexDateTimeUtc,PlanType, IndexValue FROM dbo.InvestmentIndexHistory WHERE PlanType = @PlanType 
                                 ORDER BY IndexDateTimeUtc DESC;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            var result = await conn.QueryAsync<InvestmentIndexHistory>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        PlanType = planType,
                        Count = count
                    },
                    cancellationToken: ct));

            return result.AsList();


        }
    }
}
