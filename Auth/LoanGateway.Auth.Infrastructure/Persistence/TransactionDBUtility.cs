using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanGateway.Auth.Infrastructure.Persistence
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
