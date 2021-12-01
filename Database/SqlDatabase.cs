using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ms_identity_dotnet_blazor_azure_sql.Database
{
    public class SqlDatabase
    {
        IConfiguration _configuration;

        public SqlDatabase(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection GetSqlConnection(string connStringName = "SqlDbContext")
        {
            return new(_configuration.GetConnectionString(connStringName));
        }
    }
}
