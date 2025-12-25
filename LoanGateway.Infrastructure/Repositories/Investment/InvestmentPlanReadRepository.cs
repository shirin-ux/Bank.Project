using Dapper;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using System.Data;
using System.Reflection;

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

        public async Task<InvestmentAccount?> GetByNationalCodeAndPlanAsync(string nationalCode, InvestmentPlanType planType, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 1 
                    Id, ProviderPolicyId, NationalCode, BirthDate, PlanCode,
                    PostalCode, Address, State,
                    TotalInvested, TotalWithdrawn, CurrentValue,
                    RevokableAmount, CollateralAmount, LastTraceId,
                    CreatedAtUtc, UpdatedAtUtc
                FROM dbo.InvestmentAccount
                WHERE NationalCode = @NationalCode 
                    AND PlanCode = @PlanCode
                ORDER BY CreatedAtUtc DESC;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            var flat = await conn.QueryFirstOrDefaultAsync<InvestmentAccountFlat>(
                new CommandDefinition(sql, new { NationalCode = nationalCode, PlanCode = (int)planType }, cancellationToken: ct));

            if (flat == null)
                return null;

            // استفاده از reflection برای ساخت InvestmentAccount (چون constructor private است)
            var account = Activator.CreateInstance(typeof(InvestmentAccount), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null, null) as InvestmentAccount;
            if (account == null) return null;

            // استفاده از reflection برای set کردن properties (با private setters)
            var props = typeof(InvestmentAccount).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var baseProps = typeof(BaseEntity).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            foreach (var prop in props.Concat(baseProps))
            {
                var setter = prop.GetSetMethod(true); // true برای گرفتن private setter
                if (setter != null)
                {
                    object? value = null;
                    if (prop.Name == "PlanCode" && flat.PlanCode > 0)
                    {
                        value = (InvestmentPlanType)flat.PlanCode;
                    }
                    else
                    {
                        var flatProp = flat.GetType().GetProperty(prop.Name);
                        value = flatProp?.GetValue(flat);
                    }
                    
                    if (value != null || (prop.PropertyType.IsValueType ))
                    {
                        setter.Invoke(account, new[] { value });
                    }
                }
            }

            // تبدیل State
            var stateProp = typeof(InvestmentAccount).GetProperty("State", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (stateProp != null)
            {
                var stateSetter = stateProp.GetSetMethod(true);
                stateSetter?.Invoke(account, new object[] { (InvestmentState)flat.State });
            }

            return account;
        }

        private class InvestmentAccountFlat
        {
            public Guid Id { get; set; }
            public Guid? ProviderPolicyId { get; set; }
            public string? NationalCode { get; set; }
            public string? BirthDate { get; set; }
            public int PlanCode { get; set; }
            public string? PostalCode { get; set; }
            public string? Address { get; set; }
            public int State { get; set; }
            public decimal TotalInvested { get; set; }
            public decimal TotalWithdrawn { get; set; }
            public decimal CurrentValue { get; set; }
            public decimal RevokableAmount { get; set; }
            public decimal CollateralAmount { get; set; }
            public string? LastTraceId { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
        }

        public async Task AddAsync(InvestmentAccount account, CancellationToken ct)
        {
            const string sql = @"
                INSERT INTO dbo.InvestmentAccount 
                    (Id, ProviderPolicyId, NationalCode, BirthDate, PlanCode,
                     PostalCode, Address, State,
                     TotalInvested, TotalWithdrawn, CurrentValue,
                     RevokableAmount, CollateralAmount, LastTraceId,
                     CreatedAtUtc, UpdatedAtUtc)
                VALUES 
                    (@Id, @ProviderPolicyId, @NationalCode, @BirthDate, @PlanCode,
                     @PostalCode, @Address, @State,
                     @TotalInvested, @TotalWithdrawn, @CurrentValue,
                     @RevokableAmount, @CollateralAmount, @LastTraceId,
                     @CreatedAtUtc, @UpdatedAtUtc);";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            await conn.ExecuteAsync(
                new CommandDefinition(sql, new
                {
                    account.Id,
                    ProviderPolicyId = account.ProviderPolicyId,
                    NationalCode = account.NationalCode,
                    BirthDate = account.BirthDate,
                    PlanCode = account.PlanCode.HasValue ? (int)account.PlanCode.Value : (int?)null,
                    PostalCode = account.PostalCode,
                    Address = account.Address,
                    State = (int)account.State,
                    TotalInvested = account.TotalInvested,
                    TotalWithdrawn = account.TotalWithdrawn,
                    CurrentValue = account.CurrentValue,
                    RevokableAmount = account.RevokableAmount,
                    CollateralAmount = account.CollateralAmount,
                    LastTraceId = account.LastTraceId,
                    CreatedAtUtc = account.CreatedAtUtc,
                    UpdatedAtUtc = account.UpdatedAtUtc
                }, cancellationToken: ct));

            // TODO: باید Operations را هم insert کنیم، اما فعلاً فقط Account را insert می‌کنیم
            // Operations باید در یک جدول جداگانه insert شوند
        }

        /// <summary>
        /// بررسی می‌کند که آیا operation با receiptNumber مشخص برای PolicyId مشخص وجود دارد یا نه
        /// </summary>
        public async Task<bool> CheckReceiptNumberExistsAsync(Guid? policyId, string receiptNumber, CancellationToken ct)
        {
            if (policyId == null || string.IsNullOrWhiteSpace(receiptNumber))
                return false;

            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.InvestmentOperation
                WHERE PolicyId = @PolicyId 
                    AND ReceiptNumber = @ReceiptNumber;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            var count = await conn.QueryFirstOrDefaultAsync<int>(
                new CommandDefinition(sql, new { PolicyId = policyId, ReceiptNumber = receiptNumber }, cancellationToken: ct));

            return count > 0;
        }

        public async Task<bool> UpdateAsync(InvestmentAccount account, CancellationToken ct)
        {
            const string sql = @"
                UPDATE dbo.InvestmentAccount
                SET State = @State,
                    TotalInvested = @TotalInvested,
                    TotalWithdrawn = @TotalWithdrawn,
                    CurrentValue = @CurrentValue,
                    RevokableAmount = @RevokableAmount,
                    CollateralAmount = @CollateralAmount,
                    LastTraceId = @LastTraceId,
                    PostalCode = @PostalCode,
                    Address = @Address,
                    UpdatedAtUtc = @UpdatedAtUtc
                WHERE Id = @Id;";

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            var affected = await conn.ExecuteAsync(
                new CommandDefinition(sql, new
                {
                    account.Id,
                    State = (int)account.State,
                    account.TotalInvested,
                    account.TotalWithdrawn,
                    account.CurrentValue,
                    account.RevokableAmount,
                    account.CollateralAmount,
                    account.LastTraceId,
                    account.PostalCode,
                    account.Address,
                    account.UpdatedAtUtc
                }, cancellationToken: ct));

            return affected > 0;
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

        public async Task<InvestmentPlans> GetPlanAsync(string planStatus, CancellationToken ct)
        {
            const string sql = @"SELECT * FROM InvestmentPlans where IsActive=1 And IsDelete=0 AND PlanStatus=@planStatus ORDER BY Id DESC;";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryFirstOrDefaultAsync<InvestmentPlans>(new CommandDefinition(sql, new { PlanStatus = planStatus }, cancellationToken: ct));
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
