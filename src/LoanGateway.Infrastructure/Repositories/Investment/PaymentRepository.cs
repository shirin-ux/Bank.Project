using Dapper;
using LoanGateway.Infrastructure.Utility;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;

namespace LoanService.Infrastructure.Repositories.Investment
{
    public sealed class PaymentRepository(TransactionDBUtility transactionDBUtility) : IPaymentRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility = transactionDBUtility;

        public async Task CreateAsync(InvestmentPayment row, CancellationToken ct)
        {
            const string sql = @"INSERT INTO dbo.InvestmentPayment (Id, OrderId, AmountRials, Status, IsFinal)
                                 VALUES (@Id, @OrderId, @AmountRials, @Status, @IsFinal);";
            await using var conn = _transactionDBUtility.GetSqlConnection();
            await conn.ExecuteAsync(
                new CommandDefinition(sql, row, cancellationToken: ct));


        }

        public async Task<InvestmentPayment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct)
        {
            const string sql = @"SELECT TOP 1
                                            Id, OrderId, AmountRials,
                                            Status as Status,
                                            IsFinal,
                                            RowVersion,
                                            SadadTokenProtected,
                                            SadadTokenHash
                                        FROM dbo.InvestmentPayment
                                        WHERE OrderId = @orderId;";
            await using var conn = _transactionDBUtility.GetSqlConnection();

            return await conn.QueryFirstOrDefaultAsync<InvestmentPayment>(
                new CommandDefinition(sql, new { Id = orderId }, cancellationToken: ct));
        }

        public async Task<bool> SetCallbackAsync(Guid paymentId, int callbackResCode, byte[] rowVersion, CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            var affected = await conn.ExecuteAsync(new CommandDefinition(@"UPDATE dbo.InvestmentPayment
                                                                                SET CallbackResCode = @callbackResCode,
                                                                                    CallbackAtUtc = SYSUTCDATETIME(),
                                                                                    Status = @status,
                                                                                    UpdatedAtUtc = SYSUTCDATETIME()
                                                                                WHERE Id = @paymentId AND RowVersion = @rowVersion AND IsFinal = 0;",
          new { paymentId, callbackResCode, status = (byte)PaymentStatus.CallbackReceived, rowVersion }, cancellationToken: ct));
            return affected == 1;
        }

        public async Task<bool> SetFailedAsync(Guid paymentId, int? verifyResCode, byte[] rowVersion, CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            var affected = await conn.ExecuteAsync(new CommandDefinition(@"
                                  UPDATE dbo.InvestmentPayment
                                  SET VerifyResCode = COALESCE(@verifyResCode, VerifyResCode),
                                      Status = @status,
                                      IsFinal = 1,
                                      UpdatedAtUtc = SYSUTCDATETIME()
                                  WHERE Id = @paymentId AND RowVersion = @rowVersion AND IsFinal = 0;",
             new { paymentId, verifyResCode, status = (byte)PaymentStatus.Failed, rowVersion }, cancellationToken: ct));
            return affected == 1;
        }

        public async Task<bool> SetTokenAsync(Guid paymentId, string tokenProtected, byte[] tokenHash, byte[] rowVersion, CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            var affected = await conn.ExecuteAsync(new CommandDefinition(@"
                                                                          UPDATE dbo.InvestmentPayment
                                                                          SET SadadTokenProtected = @tokenProtected,
                                                                              SadadTokenHash = @tokenHash,
                                                                              Status = @status,
                                                                              UpdatedAtUtc = SYSUTCDATETIME()
                                                                          WHERE Id = @paymentId AND RowVersion = @rowVersion;",
            new { paymentId, tokenProtected, tokenHash, status = (byte)PaymentStatus.TokenIssued, rowVersion }, cancellationToken: ct));
            return affected == 1;
        }

        public async Task<bool> SetVerifiedAsync(Guid paymentId, VerifyPayment verify, byte[] rowVersion, CancellationToken ct)
        {
            await using var conn = _transactionDBUtility.GetSqlConnection();
            var affected = await conn.ExecuteAsync(new CommandDefinition(@"
                                                                         UPDATE dbo.InvestmentPayment
                                                                         SET VerifyResCode = @VerifyResCode,
                                                                             VerifyAtUtc = SYSUTCDATETIME(),
                                                                             VerifiedAmountRials = @VerifiedAmountRials,
                                                                             RetrievalRefNo = @RetrievalRefNo,
                                                                             SystemTraceNo = @SystemTraceNo,
                                                                             Status = @status,
                                                                             IsFinal = 1,
                                                                             UpdatedAtUtc = SYSUTCDATETIME()
                                                                         WHERE Id = @paymentId AND RowVersion = @rowVersion AND IsFinal = 0;",
            new
            {
                paymentId,
                rowVersion,
                verify.VerifyResCode,
                verify.VerifiedAmountRials,
                verify.RetrievalRefNo,
                verify.SystemTraceNo,
                status = (byte)PaymentStatus.Verified
            }, cancellationToken: ct));
            return affected == 1;
        }

    }

}
