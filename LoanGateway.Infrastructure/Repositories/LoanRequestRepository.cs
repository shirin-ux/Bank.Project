using Dapper;
using Hangfire.Logging;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities;
using LoanService.Domain.Enum;
using LoanService.Domain.IRepository;
using LoanService.Domain.ValueObjects;
using LoanService.Infrastructure.RequestFlat;
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
        private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;
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

        //public async Task<LoanRequest?> GetByIdAsync(Guid id, CancellationToken ct)
        //{
        //    var sql = @"
        //               SELECT  *, InquiryRequest_Id FROM LoanRequest WHERE Id = @Id;
        //               SELECT * FROM ContractInfo WHERE LoanRequestId = @Id;
        //               SELECT * FROM InquiryInfo WHERE LoanRequestId = @Id;
        //               SELECT * FROM PayResponseInfo WHERE LoanRequestId = @Id;
        //               SELECT * FROM RepaymentSnapshot WHERE LoanRequestId = @Id;
        //               SELECT * FROM TransferInfo WHERE LoanRequestId = @Id;
        //               SELECT * FROM InstallmentStatus WHERE LoanRequestId = @Id;";

        //    using var conn = _transactionDBUtility.GetSqlConnection();
        //    await conn.OpenAsync(ct);

        //    using var multi = await conn.QueryMultipleAsync(sql, new { Id = id });


        //    var row = await conn.QuerySingleOrDefaultAsync<LoanRequest>("SELECT * FROM LoanRequest WHERE Id = @Id", new { Id = id });
        //    if (row is null) return null;

        //    // Map Aggregate Roots
        //    row.Contract = await multi.ReadSingleOrDefaultAsync<ContractInfo>();
        //    row.Inquiry = await multi.ReadSingleOrDefaultAsync<InquiryInfo>();
        //    row.PayResponse = await multi.ReadSingleOrDefaultAsync<PayResponseInfo>();
        //    row.LastRepayment = await multi.ReadSingleOrDefaultAsync<RepaymentSnapshot>();
        //    row.Transfer = await multi.ReadSingleOrDefaultAsync<TransferInfo>();
        //    row.InstallmentStatus = await multi.ReadSingleOrDefaultAsync<InstallmentStatus>();

        //    if (row.Customer != null)
        //    {
        //        row.Customer = new CustomerInfo(
        //            row.Customer.NationalCode,
        //            row.Customer.BirthDate,
        //            row.Customer.Mobile,
        //            row.Customer.PostalCode,
        //            row.Customer.Gender
        //        );

        //    }

        //    if (row.Provider != null)
        //    {

        //        row.Provider = new ProviderInfo(
        //            (BankProviderType)row.Provider.ProviderType,
        //            row.Provider.ApprovalCode,
        //            row.Provider.RequiresOtp
        //        );
        //    }

        //    if (row.LastDecision != null)
        //    {
        //        row.LastDecision = new DecisionStamp(
        //            row.LastDecision.ErrorCode,
        //            row.LastDecision.ErrorMessage,
        //            row.LastDecision.ReasonCode,
        //            row.LastDecision.ReasonMessage
        //        );
        //    }

        //    if (row.GrantRequest != null)
        //    {
        //        row.GrantRequest = new GrantRequest(
        //            row.GrantRequest.ContractId,
        //            row.GrantRequest.Status,
        //            row.GrantRequest.PayRequestId,
        //            row.GrantRequest.RequestedAmount,
        //            row.GrantRequest.SignedContractBase64
        //        );
        //    }
        //    //if (loan.InqueryRequest!=null)
        //    //{
        //        row.InqueryRequest = new InqueryRequest(row.InquiryRequest_Id);
        //   // }
        //    if (row.PayRequest != null)
        //    {

        //        row.PayRequest = new PayRequestInfo(row.PayRequest.PayRequestId, row.PayRequest.RequestedAmount);
        //    }
        //    return row;

        //}
        public async Task<LoanRequest?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var sql = @"
        SELECT State, * FROM LoanRequest WHERE Id = @Id;
        SELECT * FROM ContractInfo WHERE LoanRequestId = @Id;
        SELECT * FROM InquiryInfo WHERE LoanRequestId = @Id;
        SELECT * FROM PayResponseInfo WHERE LoanRequestId = @Id;
        SELECT * FROM RepaymentSnapshot WHERE LoanRequestId = @Id;
        SELECT * FROM TransferInfo WHERE LoanRequestId = @Id;
        SELECT * FROM InstallmentStatus WHERE LoanRequestId = @Id;";

            using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            using var multi = await conn.QueryMultipleAsync(sql, new { Id = id });

            // داده‌ی اصلی LoanRequest رو فلت بخون
            var flat = await multi.ReadSingleOrDefaultAsync<LoanRequestFlat>();

            if (flat is null)
                   return null;

            // حالا Domain Model رو بساز و ValueObjectها رو تزریق کن
            var loan = new LoanRequest()
            {
                  State=flat.State,
                 Id=id,
                Customer = new CustomerInfo(
                    flat.Customer_NationalCode,
                    flat.Customer_BirthDate,
                    flat.Customer_Mobile,
                    flat.Customer_PostalCode,
                    flat.Customer_Gender
                ),

                Provider = new ProviderInfo(
                    (BankProviderType)flat.Provider_Type,
                    flat.Provider_ApprovalCode,
                    flat.Provider_RequiresOtp
                ),

                InqueryRequest = new InqueryRequest(flat.InquiryRequest_Id),
                PayRequest = flat.PayRequest_Id != null ? new PayRequestInfo(flat.PayRequest_Id, flat.PayRequest_RequestedAmount) : null,
                LastDecision = new DecisionStamp(flat.Decision_ErrorCode, flat.Decision_ErrorMessage, flat.Decision_ReasonCode, flat.Decision_ReasonMessage),
                GrantRequest = new GrantRequest(flat.Grant_ContractId??0, flat.Grant_Status, flat.PayRequest_Id, flat.Grant_RequestedAmount, flat.Grant_SignedContractBase64),
       
            };

           
            loan.Contract = await multi.ReadSingleOrDefaultAsync<ContractInfo>();
            loan.Inquiry = await multi.ReadSingleOrDefaultAsync<InquiryInfo>();
            loan.PayResponse = await multi.ReadSingleOrDefaultAsync<PayResponseInfo>();
            loan.LastRepayment = await multi.ReadSingleOrDefaultAsync<RepaymentSnapshot>();
            loan.Transfer = await multi.ReadSingleOrDefaultAsync<TransferInfo>();
            loan.InstallmentStatus = await multi.ReadSingleOrDefaultAsync<InstallmentStatus>();

            return loan;
        }

        public async Task InsertAsync(LoanRequest loan, CancellationToken ct)
        {
            var loanSql = @"
        INSERT INTO LoanRequest (
            Id, CreatedAtUtc, UpdatedAtUtc, State, RequiresOtp, CorrelationId,
            Customer_NationalCode, Customer_BirthDate, Customer_Mobile, Customer_PostalCode, Customer_Gender,
            Provider_Type, Provider_ApprovalCode, Provider_RequiresOtp,
            Decision_ErrorCode, Decision_ErrorMessage, Decision_ReasonCode, Decision_ReasonMessage,
            Grant_ContractId, InquiryRequest_Id, PayRequest_Id,
            LastReasonCode, LastReasonMessage, LastErrorCode, LastErrorMessage
        ) VALUES (
            @Id, @CreatedAtUtc, @UpdatedAtUtc, @State, @RequiresOtp, @CorrelationId,
            @Customer_NationalCode, @Customer_BirthDate, @Customer_Mobile, @Customer_PostalCode, @Customer_Gender,
            @Provider_Type, @Provider_ApprovalCode, @Provider_RequiresOtp,
            @Decision_ErrorCode, @Decision_ErrorMessage, @Decision_ReasonCode, @Decision_ReasonMessage,
            @Grant_ContractId, @InquiryRequest_Id, @PayRequest_Id,
            @LastReasonCode, @LastReasonMessage, @LastErrorCode, @LastErrorMessage
        )";

            using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);

            using var tran = conn.BeginTransaction();

            try
            {
                // Insert LoanRequest
                await conn.ExecuteAsync(loanSql, new
                {
                    loan.Id,
                    loan.CreatedAtUtc,
                    loan.UpdatedAtUtc,
                    State = (int)loan.State,
                    loan.RequiresOtp,

                    loan.CorrelationId,

                    Customer_NationalCode = loan.Customer.NationalCode,
                    Customer_BirthDate = loan.Customer.BirthDate,
                    Customer_Mobile = loan.Customer.Mobile,
                    Customer_PostalCode = loan.Customer.PostalCode,
                    Customer_Gender = loan.Customer.Gender,

                    Provider_Type = (int)loan.Provider.ProviderType,
                    Provider_ApprovalCode = loan.Provider.ApprovalCode,
                    Provider_RequiresOtp = loan.Provider.RequiresOtp,

                    Decision_ErrorCode = loan.LastDecision.ErrorCode,
                    Decision_ErrorMessage = loan.LastDecision.ErrorMessage,
                    Decision_ReasonCode = loan.LastDecision.ReasonCode,
                    Decision_ReasonMessage = loan.LastDecision.ReasonMessage,
                  
                    Grant_ContractId = loan.GrantRequest.ContractId,
                    InquiryRequest_Id = loan.InqueryRequest.RequestId,
                    PayRequest_Id = loan.PayRequest.PayRequestId,

                    loan.LastReasonCode,
                    loan.LastReasonMessage,
                    loan.LastErrorCode,
                    loan.LastErrorMessage
                }, tran);

                // Insert Aggregate Roots
                if (loan.Contract != null)
                    await conn.ExecuteAsync("InsertContractInfo",
                        new
                        {
                            LoanRequestId = loan.Id,
                            loan.Contract.NationalCode,
                            loan.Contract.CollateralType,
                            loan.Contract.CollateralNo,
                            loan.Contract.Address,
                            loan.Contract.ApprovalCode,
                            loan.Contract.ChequeSerial,
                            loan.Contract.CollateralAmount,
                            loan.Contract.CollateralDate,
                            loan.Contract.CollateralIssuer,
                            loan.Contract.GuarantorNC,
                            loan.Contract.InstallmentCount,
                            loan.Contract.LoanAmount,
                            loan.Contract.MobileNumber,
                            loan.Contract.PhoneNumber,
                            loan.Contract.PostalCode,
                            loan.Contract.BirthDate,
                            loan.Contract.cbTrackingCode,
                            loan.Contract.ContractPath

                        }, tran);

                if (loan.Inquiry != null)
                    await conn.ExecuteAsync(@"
                INSERT INTO InquiryInfo (LoanRequestId, Allowed, MaxApprovedAmount, Ics, IcsGrade, ExpireAt)
                VALUES (@LoanRequestId, @Allowed, @MaxApprovedAmount, @Ics, @IcsGrade, @ExpireAt)",
                        new { LoanRequestId = loan.Id, loan.Inquiry.Allowed, loan.Inquiry.MaxApprovedAmount, loan.Inquiry.Ics, loan.Inquiry.IcsGrade, loan.Inquiry.ExpireAt }, tran);

                if (loan.PayResponse != null)
                    await conn.ExecuteAsync(@"
                INSERT INTO PayResponseInfo (LoanRequestId, Code, BankContractNo, ApprovedLoanAmount, ContractDate, CentralBankTraceCode, BankSignedContractBase64, ReceivedAtUtc)
                VALUES (@LoanRequestId, @Code, @BankContractNo, @ApprovedLoanAmount, @ContractDate, @CentralBankTraceCode, @BankSignedContractBase64, @ReceivedAtUtc)",
                        new { LoanRequestId = loan.Id, Code = (int)loan.PayResponse.Code, loan.PayResponse.BankContractNo, loan.PayResponse.ApprovedLoanAmount, loan.PayResponse.ContractDate, loan.PayResponse.CentralBankTraceCode, loan.PayResponse.BankSignedContractBase64, loan.PayResponse.ReceivedAtUtc }, tran);

                if (loan.LastRepayment != null)
                    await conn.ExecuteAsync(@"
                INSERT INTO RepaymentSnapshot (LoanRequestId, TrackNumber, AccountNo, Amount, WhenUtc)
                VALUES (@LoanRequestId, @TrackNumber, @AccountNo, @Amount, @WhenUtc)",
                        new { LoanRequestId = loan.Id, loan.LastRepayment.TrackNumber, loan.LastRepayment.AccountNo, loan.LastRepayment.Amount, loan.LastRepayment.WhenUtc }, tran);

                if (loan.Transfer != null)
                    await conn.ExecuteAsync(@"
                INSERT INTO TransferInfo (LoanRequestId, TransactionNumber, RegisterCode)
                VALUES (@LoanRequestId, @TransactionNumber, @RegisterCode)",
                        new { LoanRequestId = loan.Id, loan.Transfer.TransactionNumber, loan.Transfer.RegisterCode }, tran);

                if (loan.InstallmentStatus != null)
                    await conn.ExecuteAsync(@"
                INSERT INTO InstallmentStatus (LoanRequestId, Status, DueDate, PaidAmount)
                VALUES (@LoanRequestId, @Status, @DueDate, @PaidAmount)",
                        new { LoanRequestId = loan.Id, loan.InstallmentStatus.Status, loan.InstallmentStatus.DueDate, loan.InstallmentStatus.PaidAmount }, tran);

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }



        public async Task InsertInstallmentAsync(List<InstallmentStatus> installmentStatus, Guid loanId, CancellationToken ct)
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


                });
            }
        }

        public async Task UpdateAsync(LoanRequest loan, CancellationToken ct)
        {
            using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.OpenAsync(ct);
            using var tran = conn.BeginTransaction();
            try
            {
                // -------- LoanRequest --------
                var loanSql = @"
                UPDATE LoanRequest SET
                    UpdatedAtUtc = @UpdatedAtUtc,
                    State = @State,
                    RequiresOtp = @RequiresOtp,
                    Customer_NationalCode=@Customer_NationalCode,
                    Customer_BirthDate=@Customer_BirthDate,
                    Customer_Mobile=@Customer_Mobile,
                    Customer_PostalCode=@Customer_PostalCode,
                    Customer_Gender=@Customer_Gender,
                    Provider_Type=@Provider_Type,
                    Provider_ApprovalCode=@Provider_ApprovalCode,
                    Provider_RequiresOtp=@Provider_RequiresOtp,
                    Decision_ErrorCode=@Decision_ErrorCode,
                    Decision_ErrorMessage=@Decision_ErrorMessage,
                    Decision_ReasonCode=@Decision_ReasonCode,
                    Decision_ReasonMessage=@Decision_ReasonMessage,
                    Grant_ContractId=@Grant_ContractId,
                    InquiryRequest_Id=@InquiryRequest_Id,
                    PayRequest_Id=@PayRequest_Id,
                    LastReasonCode=@LastReasonCode,
                    LastReasonMessage=@LastReasonMessage,
                    LastErrorCode=@LastErrorCode,
                    LastErrorMessage=@LastErrorMessage
                WHERE Id=@Id";

                await conn.ExecuteAsync(loanSql, new
                {
                    loan.Id,
                    loan.UpdatedAtUtc,
                    State = (int)loan.State,
                    loan.RequiresOtp,

                    Customer_NationalCode = loan.Customer?.NationalCode,
                    Customer_BirthDate = loan.Customer?.BirthDate,
                    Customer_Mobile = loan.Customer?.Mobile,
                    Customer_PostalCode = loan.Customer?.PostalCode,
                    Customer_Gender = loan.Customer?.Gender,

                    Provider_Type = (int)loan.Provider?.ProviderType,
                    Provider_ApprovalCode = loan.Provider?.ApprovalCode,
                    Provider_RequiresOtp = loan.Provider?.RequiresOtp,

                    Decision_ErrorCode = loan.LastDecision?.ErrorCode,
                    Decision_ErrorMessage = loan.LastDecision?.ErrorMessage,
                    Decision_ReasonCode = loan.LastDecision?.ReasonCode,
                    Decision_ReasonMessage = loan.LastDecision?.ReasonMessage,

                    Grant_ContractId = loan.GrantRequest?.ContractId,
                    InquiryRequest_Id = loan.InqueryRequest?.RequestId,
                    PayRequest_Id = loan.PayRequest?.PayRequestId,

                    loan.LastReasonCode,
                    loan.LastReasonMessage,
                    loan.LastErrorCode,
                    loan.LastErrorMessage
                }, tran);

                // -------- Aggregate Roots --------
                if (loan.Contract is not null)
                {
                    await conn.ExecuteAsync("sp_ContractInfo_Upsert", new
                    {
                        LoanRequestId = loan.Id,
                        loan.Contract.CollateralNo,
                        loan.Contract.CollateralType,
                        loan.Contract.CollateralDate,
                        loan.Contract.ApprovalCode,
                        loan.Contract.CollateralAmount,
                        loan.Contract.Address,
                        loan.Contract.BirthDate,
                        loan.Contract.cbTrackingCode,
                        loan.Contract.ChequeSerial,
                        loan.Contract.CollateralIssuer,
                        loan.Contract.GuarantorNC,
                        loan.Contract.NationalCode,
                        loan.Contract.InstallmentCount,
                        loan.Contract.LoanAmount,
                        loan.Contract.MobileNumber,
                        loan.Contract.PhoneNumber,
                        loan.Contract.PostalCode,
                        loan.Contract.ContractPath
                    }, tran, commandType: CommandType.StoredProcedure);
                }
                if (loan.Inquiry is not null)
                {
                    await conn.ExecuteAsync("sp_InquiryInfo_Upsert", new
                    {
                        LoanRequestId = loan.Id,
                        loan.Inquiry.Allowed,
                        loan.Inquiry.MaxApprovedAmount,
                        loan.Inquiry.Ics,
                        loan.Inquiry.IcsGrade,
                        loan.Inquiry.ExpireAt

                    }, tran, commandType: CommandType.StoredProcedure);
                }
                if (loan.PayResponse is not null)
                {
                    await conn.ExecuteAsync("sp_PayResponseInfo_Upsert", new
                    {
                        LoanRequestId = loan.Id,
                        Code = (int)loan.PayResponse.Code,
                        loan.PayResponse.BankContractNo,
                        loan.PayResponse.ApprovedLoanAmount,
                        loan.PayResponse.ContractDate,
                        loan.PayResponse.CentralBankTraceCode,
                        loan.PayResponse.BankSignedContractBase64,
                        loan.PayResponse.ReceivedAtUtc

                    }, tran, commandType: CommandType.StoredProcedure);
                }

                if (loan.Transfer is not null)
                {
                    await conn.ExecuteAsync("sp_TransferInfo_Upsert", new
                    {
                        LoanRequestId = loan.Id,
                        loan.Transfer.TransactionNumber,
                        loan.Transfer.RegisterCode

                    }, tran, commandType: CommandType.StoredProcedure);
                }
                if (loan.LastRepayment is not null)
                {
                    await conn.ExecuteAsync("sp_RepaymentSnapshot_Upsert", new
                    {
                        LoanRequestId = loan.Id,
                        loan.LastRepayment.TrackNumber,
                        loan.LastRepayment.AccountNo,
                        loan.LastRepayment.Amount,
                        loan.LastRepayment.WhenUtc

                    }, tran, commandType: CommandType.StoredProcedure);
                }
                if (loan.InstallmentStatus is not null)
                {
                    await conn.ExecuteAsync("sp_InstallmentStatus_Upsert", new
                    {
                        LoanRequestId = loan.Id,
                        loan.InstallmentStatus.Status,
                        loan.InstallmentStatus.DueDate,
                        loan.InstallmentStatus.PaidAmount,
                        loan.InstallmentStatus.NationalCode,
                        loan.InstallmentStatus.ContractNumber,


                    }, tran, commandType: CommandType.StoredProcedure);
                }
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }


        //private async Task UpsertAsync(SqlConnection conn, SqlTransaction tran, string tableName, Guid loanId, object parameters)
        //{


        //    var exists = await conn.ExecuteScalarAsync<int>(
        //        $"SELECT COUNT(1) FROM {tableName} WHERE LoanRequestId=@LoanRequestId",
        //        new { LoanRequestId = loanId }, tran);

        //    var props = parameters.GetType().GetProperties();
        //    var hasLoanRequestId = props.Any(p => p.Name.Equals("LoanRequestId", StringComparison.OrdinalIgnoreCase));

        //    var dynamicParams = new DynamicParameters(parameters);
        //    if (!hasLoanRequestId)
        //        dynamicParams.Add("LoanRequestId", loanId);

        //    var columns = props.Select(p => p.Name).ToList();
        //    var values = props.Select(p => "@" + p.Name).ToList();

        //    if (!hasLoanRequestId)
        //    {
        //        columns.Insert(0, "LoanRequestId");
        //        values.Insert(0, "@LoanRequestId");
        //    }

        //    if (exists > 0)
        //    {
        //        var setClause = string.Join(", ", props
        //            .Where(p => !p.Name.Equals("LoanRequestId", StringComparison.OrdinalIgnoreCase))
        //            .Select(p => $"{p.Name}=@{p.Name}"));
        //        var sql = $"UPDATE {tableName} SET {setClause} WHERE LoanRequestId=@LoanRequestId";
        //        await conn.ExecuteAsync(sql, dynamicParams, tran);
        //    }
        //    else
        //    {
        //        var sql = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        //        await conn.ExecuteAsync(sql, dynamicParams, tran);
        //    }
        //}
    }
}

