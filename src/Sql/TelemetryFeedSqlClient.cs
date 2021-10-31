using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace TimHanewich.TelemetryFeed.Sql
{
    public class TelemetryFeedSqlClient
    {
        //Saved internal variables
        private string SqlConnectionString;

        public TelemetryFeedSqlClient(string sql_connection_string)
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
    
    
    
        //SQL transactions

        public async Task UploadRegisteredUserAsync(RegisteredUser user)
        {
            await ExecuteNonQueryAsync(user.ToSqlInsert());
        }

        public async Task UploadSessionAsync(Session s)
        {
            await ExecuteNonQueryAsync(s.ToSqlInsert());
        }

        public async Task UploadTelemetrySnapshotAsync(TelemetrySnapshot ts)
        {
            await ExecuteNonQueryAsync(ts.ToSqlInsert());
        }
    
        public async Task<RegisteredUser> DownloadRegisteredUserAsync(string username)
        {
            string cmd = "select Id, Username, Password from RegisteredUser where Username = '" + username + "'";
            SqlConnection sqlcon = GetSqlConnection();
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            if (dr.HasRows == false)
            {
                sqlcon.Close();
                throw new Exception("Unable to find user with username '" + username + "'");
            }
            dr.Read();
            RegisteredUser ToReturn = ExtractRegisteredUserFromSqlDataReader(dr);
            sqlcon.Close();
            return ToReturn;
        }

        public RegisteredUser ExtractRegisteredUserFromSqlDataReader(SqlDataReader dr, string prefix = "")
        {
            RegisteredUser ru = new RegisteredUser();

            //Id
            try
            {
                ru.Id = dr.GetGuid(dr.GetOrdinal(prefix + "Id"));
            }
            catch
            {
                
            }

            //Username
            try
            {
                ru.Username = dr.GetString(dr.GetOrdinal(prefix + "Username"));
            }
            catch
            {

            }

            //Password
            try
            {
                ru.Password = dr.GetString(dr.GetOrdinal(prefix + "Password"));
            }
            catch
            {

            }

            return ru;
        }

        public async Task <Session[]> DownloadSessionsAsync(Guid owner_id)
        {
            string cmd = "select Id,Owner,Title,CreatedAtUtc,RightLeanCalibration,LeftLeanCalibration from Session where Owner = '" + owner_id.ToString() + "' order by CreatedAtUtc desc";
            SqlConnection sqlcon = GetSqlConnection();
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            List<Session> ToReturn = new List<Session>();
            while (dr.Read())
            {
                ToReturn.Add(ExtractSessionFromSqlDataReader(dr));
            }
            sqlcon.Close();
            return ToReturn.ToArray();
        }

        public Session ExtractSessionFromSqlDataReader(SqlDataReader dr, string prefix = "")
        {
            Session s = new Session();
            
            //Id
            try
            {
                s.Id = dr.GetGuid(dr.GetOrdinal(prefix + "Id"));
            }
            catch
            {

            }

            //Owner
            try
            {
                s.Owner = dr.GetGuid(dr.GetOrdinal(prefix + "Owner"));
            }
            catch
            {

            }

            //Title
            try
            {
                s.Title = dr.GetString(dr.GetOrdinal(prefix + "Title"));
            }
            catch
            {

            }

            //CreatedAtUtc
            try
            {
                s.CreatedAtUtc = dr.GetDateTime(dr.GetOrdinal(prefix + "CreatedAtUtc"));
            }
            catch
            {

            }

            //RightLeanCalibration
            try
            {
                s.RightLeanCalibration = dr.GetGuid(dr.GetOrdinal(prefix + "RightLeanCalibration"));
            }
            catch
            {

            }

            //LeftLeanCalibration
            try
            {
                s.LeftLeanCalibration = dr.GetGuid(dr.GetOrdinal(prefix + "LeftLeanCalibration"));
            }
            catch
            {

            }
            
            return s;
        }


        //SQL checks

        public async Task<bool> RegisteredUserExistsAsync(Guid id)
        {
            string cmd = "select count(Id) from RegisteredUser where Id = '" + id.ToString() + "'";
            SqlConnection sqlcon = GetSqlConnection();
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            await dr.ReadAsync();
            int val = dr.GetInt32(0);
            sqlcon.Close();
            if (val > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    
    
    
    
    
    
    }
}