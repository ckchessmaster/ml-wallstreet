using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataManagerAPI.Utils
{
    public class SqlHelper
    {
        private readonly IConfiguration config;

        public SqlHelper(IConfiguration config)
        {
            this.config = config;
        }

        public SqlConnection GetSqlConnection()
        {
            return new SqlConnection(config.GetConnectionString("DataManagerConnectionString"));
        }
    }
}
