using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities.Loan;
using LoanService.Domain.IRepository.Loan;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace LoanService.Infrastructure.Repositories.Loan;

public class InstallmentRepository(TransactionDBUtility transactionDBUtility) : IInstallmentRepository
{
    private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;
    public async Task<int> InsertAsync(InstallmentStatus entity, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO Installments
            (Id, LoanRequestId, ContractNumber, InstallmentNo, NationalCode, DueDate, Amount, PaidAmount, [Status])
            VALUES
            (@Id, @LoanRequestId, @ContractNumber, @InstallmentNo, @NationalCode, @DueDate, @Amount, @PaidAmount, @Status)";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);
        return await conn.ExecuteAsync(sql, entity);
    }

    public async Task<int> UpdateAsync(InstallmentStatus entity, CancellationToken ct)
    {
        const string sql = @"
            UPDATE Installments
            SET
                LoanRequestId = @LoanRequestId,
                ContractNumber = @ContractNumber,
                InstallmentNo = @InstallmentNo,
                NationalCode = @NationalCode,
                DueDate = @DueDate,
                Amount = @Amount,
                PaidAmount = @PaidAmount,
                [Status] = @Status
            WHERE Id = @Id";


        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);
        return await conn.ExecuteAsync(sql, entity);
    }

    public async Task<InstallmentStatus?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT * FROM Installments WHERE Id = @Id";

        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);
       
        return await conn.QuerySingleOrDefaultAsync<InstallmentStatus>(sql, new { Id = id });
    }

    public async Task<IEnumerable<InstallmentStatus>> GetByLoanRequestIdAsync(Guid loanRequestId, CancellationToken ct)
    {
        const string sql = "SELECT * FROM Installments WHERE LoanRequestId = @LoanRequestId ORDER BY InstallmentNo ASC";
        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);
        return await conn.QueryAsync<InstallmentStatus>(sql, new { LoanRequestId = loanRequestId });
    }

    public async Task<IEnumerable<InstallmentStatus>> GetAllAsync(CancellationToken ct)
    {
        const string sql = "SELECT * FROM Installments ORDER BY DueDate ASC";
        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);
        return await conn.QueryAsync<InstallmentStatus>(sql);
    }

    public async Task<int> DeleteAsync(Guid id, CancellationToken ct)
    {
        const string sql = "DELETE FROM Installments WHERE Id = @Id";
        await using var conn = _transactionDBUtility.GetSqlConnection();
        await conn.OpenAsync(ct);
        return await conn.ExecuteAsync(sql, new { Id = id });
    }
}

