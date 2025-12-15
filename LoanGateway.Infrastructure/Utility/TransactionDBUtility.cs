using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LoanGateway.Infrastructure.Utility
{
    public class TransactionDBUtility
    {
        private IConfiguration configuration;
   
        private readonly string _transactionDbConn;
        private readonly string _transactionDb1Conn;

        public TransactionDBUtility(IConfiguration configuration)
        {
            _transactionDbConn = configuration.GetConnectionString("TransactionDB")
                ?? throw new InvalidOperationException("Connection string 'TransactionDB' not found.");

            _transactionDb1Conn = configuration.GetConnectionString("TransactionDB1")
                ?? throw new InvalidOperationException("Connection string 'TransactionDB1' not found.");
        }

        public SqlConnection GetSqlConnection()
            => new SqlConnection(_transactionDbConn);

        public SqlConnection GetSqlConnection1()
            => new SqlConnection(_transactionDb1Conn);


        //public TransactionDBUtility(IConfiguration configuration)
        //{
        //    this.configuration = configuration;
        //    strConn = configuration.GetConnectionString("TransactionDB");
        //    if (string.IsNullOrWhiteSpace(strConn))
        //        throw new InvalidOperationException("Connection string 'TransactionDB' not found or empty!");

        //    strConn = configuration.GetConnectionString("TransactionDB1");
        //    if (string.IsNullOrWhiteSpace(strConn))
        //        throw new InvalidOperationException("Connection string 'TransactionDB' not found or empty!");

        //}

        //public SqlConnection GetSqlConnection()
        //{
        //    return new SqlConnection(strConn);
        //}

    }
}
