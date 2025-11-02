using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities;
using LoanService.Domain.IRepository;
using LoanService.Domain.ValueObjects;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using static Hangfire.Storage.JobStorageFeatures;


namespace LoanService.Infrastructure.Repositories
{
    public class LoanRequestRepository(TransactionDBUtility transactionDBUtility) : ILoanRequestRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility= transactionDBUtility;
        public async Task<LoanRequest?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct)
        {
            const string sql = @"SELECT TOP(1) Id, Provider, ProductCode, Amount, NationalId, Mobile, WithCollateral, MerchantId,
                                 State, CreatedAtUtc, UpdatedAtUtc, ContractId, LastErrorCode, LastErrorMessage, CorrelationId, IdempotencyKey
                                 FROM dbo.LoanRequests
                                 WHERE IdempotencyKey = @IdempotencyKey;";

            var args = new { IdempotencyKey = idempotencyKey };


            await using var conn = _transactionDBUtility.GetSqlConnection();
  
            await conn.OpenAsync(ct);
            return await conn.QueryFirstOrDefaultAsync<LoanRequest>(
                new CommandDefinition(sql, args, cancellationToken: ct));
        }

        public async Task<LoanRequest?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            const string sql = @"SELECT TOP(1)Id, Provider, ProductCode, Amount, NationalId, Mobile, WithCollateral, MerchantId, State, CreatedAtUtc, UpdatedAtUtc, 
                                 ContractId, LastErrorCode, LastErrorMessage, CorrelationId, IdempotencyKey
                                 FROM dbo.LoanRequests
                                   WHERE Id = @Id;";

       
            await using var conn = _transactionDBUtility.GetSqlConnection();
            return await conn.QueryFirstOrDefaultAsync<LoanRequest>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        }

        public async Task InsertAsync(LoanRequest loan, CancellationToken ct)
        {
            var sql = @"
INSERT INTO LoanRequests (
    Id, CreatedAtUtc, UpdatedAtUtc, State, Provider, Customer, RequiresOtp,
    RowVersion, CorrelationId, InqueryRequest, Facility, Inquiry, Contract, 
    PayRequest, PayResponse, Transfer, LastRepayment, LastDecision,
    LastReasonCode, LastReasonMessage, LastErrorCode, LastErrorMessage
) VALUES (
    @Id, @CreatedAtUtc, @UpdatedAtUtc, @State, @Provider, @Customer, @RequiresOtp,
    @RowVersion, @CorrelationId, @InqueryRequest, @Facility, @Inquiry, @Contract,
    @PayRequest, @PayResponse, @Transfer, @LastRepayment, @LastDecision,
    @LastReasonCode, @LastReasonMessage, @LastErrorCode, @LastErrorMessage
)";

           await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            await conn.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                State = (int)LoanRequestState.Requested,
                //Provider = JsonSerializer.Serialize(loan.Provider),
                //Customer = JsonSerializer.Serialize(loan.Customer),
                //RequiresOtp = loan.RequiresOtp,
                //RowVersion = 1,
                //CorrelationId = loan.CorrelationId,
                //InqueryRequest = JsonSerializer.Serialize(loan.InqueryRequest),
                //Facility = JsonSerializer.Serialize(loan.GrantRequest),
                //Inquiry = JsonSerializer.Serialize(loan.Inquiry),
                //Contract = JsonSerializer.Serialize(loan.Contract),
                //PayRequest = JsonSerializer.Serialize(loan.PayRequest),
                //PayResponse = JsonSerializer.Serialize(loan.PayResponse),
                //Transfer = JsonSerializer.Serialize(loan.Transfer),
                //LastRepayment = JsonSerializer.Serialize(loan.LastRepayment),
                //LastDecision = JsonSerializer.Serialize(loan.LastDecision),
                LastReasonCode = loan.LastReasonCode,
                LastReasonMessage = loan.LastReasonMessage,
                LastErrorCode = loan.LastErrorCode,
                LastErrorMessage = loan.LastErrorMessage
            });

        }

        public  async Task InsertInstallmentAsync(List<InstallmentStatus> installmentStatus,Guid loanId, CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            await conn.ExecuteAsync("DELETE FROM [dbo].[InstallmentStatus] WHERE RequestId = @RequestId", new { RequestId = loanId });

       
            const string insertSql = @"INSERT INTO [dbo].[InstallmentStatus]
                                     ([RequestId], [InstallmentNo], [DueDate], [Amount], [PaidAmount], [Status], [LastUpdatedAt])
                                     VALUES
                                    (@RequestId, @InstallmentNo, @DueDate, @Amount, @PaidAmount, @Status, SYSUTCDATETIME());";

            foreach (var item in installmentStatus)
            {
                await conn.ExecuteAsync(insertSql, new
                {
                    Id = Guid.NewGuid(),
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    State = item.Status,
                    Amount = item.Amount,
                    DueDate = item.DueDate,
                    PaidAmount = item.PaidAmount,
                    RowVersion = 1

                });
            }
        }

        public async Task UpdateAsync(LoanRequest loan, CancellationToken ct)
        {
            const string sql = @"
UPDATE LoanRequests SET
    State = @State,
    LastReasonCode = @LastReasonCode,
    LastReasonMessage = @LastReasonMessage,
    LastErrorCode = @LastErrorCode,
    LastErrorMessage = @LastErrorMessage,
    UpdatedAtUtc = @UpdatedAtUtc,
    NationalCode = @NationalCode,
    Mobile = @Mobile,
    CustomerAccountNo = @CustomerAccountNo,
    CorrelationId = @CorrelationId,

    InquiryRequestId = @InquiryRequestId,
    Allowed = @Allowed,
    MaxApprovedAmount = @MaxApprovedAmount,
    Ics = @Ics,
    IcsGrade = @IcsGrade,
    RequestExpireDate = @RequestExpireDate,
    ContractNumber = @ContractNumber,
    ContractDesc = @ContractDesc,
    ContractWithCollateral = @ContractWithCollateral,
    PayRequestId = @PayRequestId,
    ApprovedLoanAmount = @ApprovedLoanAmount,
    ContractDate = @ContractDate,
    CentralBankTraceCode = @CentralBankTraceCode,
    BankSignedContractBase64 = @BankSignedContractBase64,
    SignedContractBase64 = @SignedContractBase64,
    ApprovalCode = @ApprovalCode,
    RequestedAmount = @RequestedAmount,
    RegisterCode = @RegisterCode,
    TransactionNumber = @TransactionNumber,
    LastRepaymentTrackNumber = @LastRepaymentTrackNumber,
    LastRepaymentAccountNo = @LastRepaymentAccountNo,
    LastRepaymentAmount = @LastRepaymentAmount,
    LastRepaymentAtUtc = @LastRepaymentAtUtc,
    RequiresOtp = @RequiresOtp,
    RowVersion = RowVersion + 1
                WHERE Id = @Id AND RowVersion = @RowVersion;";

            var param = new
            {
                loan.Id,
                State = loan.State.ToString(),
                loan.LastReasonCode,
                loan.LastReasonMessage,
                loan.LastErrorCode,
                loan.LastErrorMessage,
                loan.UpdatedAtUtc,

                loan.Customer.NationalCode,
                loan.Customer.Mobile,
                loan.Customer.BirthDate,
                loan.CorrelationId,
                loan.Inquiry.Allowed,
                loan.Inquiry.MaxApprovedAmount,
                loan.Inquiry.Ics,
                loan.Inquiry.IcsGrade,
                loan.Inquiry.ExpireAt,

                ContractNumber = loan.Contract?.ContractNumber,
                ContractDesc = loan.Contract?.Desc,
                ContractWithCollateral = loan.Contract?.WithCollateral ?? false,
                loan.PayRequest.PayRequestId,
                loan.PayRequest.RequestedAmount,
                loan.PayResponse.ContractDate,
                loan.PayResponse.CentralBankTraceCode,
                loan.PayResponse.BankSignedContractBase64,
                loan.Contract.SignedContractBase64,
                loan.Contract.ApprovalCode,
                loan.PayResponse.BankContractNo,
                loan.PayResponse.ApprovedLoanAmount,
             
                loan.Transfer.RegisterCode,
                loan.Transfer.TransactionNumber,

                loan.LastRepayment.TrackNumber,
                loan.LastRepayment.AccountNo,
                loan.LastRepayment.Amount,
                loan.LastRepayment.WhenUtc,

                loan.RequiresOtp,

                loan.RowVersion, // ← از DB می‌خوانی و اینجا می‌فرستی
            };

            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            // توجه: Dapper.ExecuteAsync پارامتر CancellationToken ندارد؛ از CommandDefinition استفاده کن:
            var cmd = new CommandDefinition(sql, param, transaction: null, cancellationToken: ct);
            var affected = await conn.ExecuteAsync(cmd);

            if (affected == 0)
                throw new DBConcurrencyException("Optimistic concurrency conflict (RowVersion mismatch).");
        
        }


    }
}
