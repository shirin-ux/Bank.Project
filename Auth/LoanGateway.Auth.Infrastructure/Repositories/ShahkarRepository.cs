using Dapper;
using LoanGateway.Auth.Domain.Entities;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Infrastructure.Repositories
{
    public class ShahkarRepository : IShahkarRepository
    {
        private readonly TransactionDBUtility _transactionDBUtility;
        public ShahkarRepository(TransactionDBUtility transactionDBUtility)
        {
            _transactionDBUtility = transactionDBUtility; 
        }
        public async Task InsertAsync(VerfiyMobileOwnerInquiry log, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                                 INSERT INTO [dbo].[VerfiyMobileOwnerInquiry]
                                 (
                                     Id,
                                     NationalId,
                                     MobileNumber,
                                     IsMatched,
                                     StatusCode,
                                     StatusMessage,
                                     RequestId,
                                     CorrelationId,
                                     RawResponseJson
                                 )
                                 VALUES
                                 (
                                     @Id,
                                     @NationalId,
                                     @MobileNumber,
                                     @IsMatched,
                                     @StatusCode,
                                     @StatusMessage,
                                     @RequestId,
                                     @CorrelationId,
                                     @RawResponseJson
                                 );";

            if (log.Id == Guid.Empty)
                log.Id = Guid.NewGuid();

            await using var conn =  _transactionDBUtility.GetSqlConnection();
            await conn.ExecuteAsync(new CommandDefinition(
                sql,
                log,
                cancellationToken: cancellationToken));
        }
    }
}

