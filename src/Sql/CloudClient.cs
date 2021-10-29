using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace TimHanewich.TelemetryFeed.Sql
{
    public class CloudClient
    {
        //Saved internal variables
        private string SqlConnectionString;

        public CloudClient(string sql_connection_string)
        {
            SqlConnectionString = sql_connection_string;
        }

        public SqlConnection GetSqlConnection()
        {
            SqlConnection sqlcon = new SqlConnection(SqlConnectionString);
            return sqlcon;
        }
    
        public async Task ExecuteNonQueryAsync(string query)
        {
            SqlConnection sqlcon = GetSqlConnection();
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
            await sqlcmd.ExecuteNonQueryAsync();
            sqlcon.Close();
        }
    }
}