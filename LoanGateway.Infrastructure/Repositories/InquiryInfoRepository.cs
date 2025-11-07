using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Infrastructure.Repositories
{
    public class InquiryInfoRepository(TransactionDBUtility transactionDBUtility) : IInquiryInfoRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;
        public async Task<int> InsertAsync(InquiryInfo entity, CancellationToken ct)
        {
            const string sql = @"
            INSERT INTO InquiryInfos
            (Id, Allowed, MaxApprovedAmount, Ics, IcsGrade, ExpireAt, LoanRequestId)
            VALUES
            (@Id, @Allowed, @MaxApprovedAmount, @Ics, @IcsGrade, @ExpireAt, @LoanRequestId)";

 
            var param = new
            {
                entity.Id,
                entity.Allowed,
                entity.MaxApprovedAmount,
                entity.Ics,
                IcsGrade = (int?)entity.IcsGrade,
                entity.ExpireAt,
                entity.LoanRequestId
            };

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            return await conn.ExecuteAsync(sql, entity);
        }

        public async Task<int> UpdateAsync(InquiryInfo entity, CancellationToken ct)
        {
            const string sql = @"
            UPDATE InquiryInfos
            SET
                Allowed = @Allowed,
                MaxApprovedAmount = @MaxApprovedAmount,
                Ics = @Ics,
                IcsGrade = @IcsGrade,
                ExpireAt = @ExpireAt,
                LoanRequestId = @LoanRequestId
            WHERE Id = @Id";

            var param = new
            {
                entity.Id,
                entity.Allowed,
                entity.MaxApprovedAmount,
                entity.Ics,
                IcsGrade = (int?)entity.IcsGrade,
                entity.ExpireAt,
                entity.LoanRequestId
            };

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            return await conn.ExecuteAsync(sql, entity);
        }

        public async Task<InquiryInfo?> GetByIdAsync(Guid id,CancellationToken ct)
        {
            const string sql = "SELECT * FROM InquiryInfos WHERE Id = @Id";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            var result = await conn.QuerySingleOrDefaultAsync<InquiryInfo>(sql, new { Id = id });
            return result;
        }

        public async Task<IEnumerable<InquiryInfo>> GetAllAsync( CancellationToken ct)
        {
            const string sql = "SELECT * FROM InquiryInfos ORDER BY ExpireAt DESC";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            return await conn.QueryAsync<InquiryInfo>(sql);
        }

        public async Task<int> DeleteAsync(Guid id, CancellationToken ct)
        {
            const string sql = "DELETE FROM Contracts WHERE Id = @Id";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            return await conn.ExecuteAsync(sql, new { Id = id });
        }

    }
}
