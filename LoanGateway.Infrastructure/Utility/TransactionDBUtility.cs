using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LoanGateway.Infrastructure.Utility
{
    public class TransactionDBUtility
    {
        private IConfiguration configuration;
        private readonly string strConn;

        public TransactionDBUtility(IConfiguration configuration)
        {
            this.configuration = configuration;
            strConn = configuration.GetConnectionString("TransactionDB");
            if (string.IsNullOrWhiteSpace(strConn))
                throw new InvalidOperationException("Connection string 'TransactionDB' not found or empty!");
  
        }

        public SqlConnection GetSqlConnection()
        {
            return new SqlConnection(strConn);
        }
    }
}
