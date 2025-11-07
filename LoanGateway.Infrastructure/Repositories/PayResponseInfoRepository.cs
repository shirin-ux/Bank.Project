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
    public class PayResponseInfoRepository(TransactionDBUtility transactionDBUtility) : IPayResponseInfoRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;
        public async Task<int> InsertAsync(PayResponseInfo entity,CancellationToken ct)
        {
            const string sql = @"
            INSERT INTO Contracts
            (Id, RequestId, ApprovalCode, WithCollateral, ContractNumber, [Desc], SignedContractBase64)
            VALUES (@Id, @RequestId, @ApprovalCode, @WithCollateral, @ContractNumber, @Desc, @SignedContractBase64)";


            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
           return await conn.ExecuteAsync(sql, entity);

        }

        public async Task<int> UpdateAsync(PayResponseInfo entity, CancellationToken ct)
        {
            const string sql = @"
            UPDATE Contracts
            SET RequestId = @RequestId,
                ApprovalCode = @ApprovalCode,
                WithCollateral = @WithCollateral,
                ContractNumber = @ContractNumber,
                [Desc] = @Desc,
                SignedContractBase64 = @SignedContractBase64
            WHERE Id = @Id";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            return await conn.ExecuteAsync(sql, entity);
      
        }

        public async Task<PayResponseInfo?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            const string sql = "SELECT * FROM Contracts WHERE Id = @Id";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            return await conn.QuerySingleOrDefaultAsync<PayResponseInfo>(sql, new { Id = id });
        }

        public async Task<IEnumerable<PayResponseInfo>> GetAllAsync(CancellationToken ct)
        {
            const string sql = "SELECT * FROM Contracts ORDER BY ContractNumber DESC";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            return await conn.QueryAsync<PayResponseInfo>(sql);
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
