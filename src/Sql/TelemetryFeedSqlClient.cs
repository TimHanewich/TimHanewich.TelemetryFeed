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
            string cmd = CoreSqlExtensions.DownloadRegisteredUser(username);
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
            string cmd = CoreSqlExtensions.DownloadSessions(owner_id);
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



        //SQL Downloads
        
        //Will download in order from newest to oldest
        public async Task<TelemetrySnapshot[]> DownloadTelemetrySnapshotsAsync(Guid from_session, int top = 20)
        {
            string cmd = CoreSqlExtensions.DownloadTelemetrySnapshots(from_session, top);
            SqlConnection sqlcon = GetSqlConnection();
            sqlcon.Open();
            SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            SqlDataReader dr = await sqlcmd.ExecuteReaderAsync();
            List<TelemetrySnapshot> ToReturn = new List<TelemetrySnapshot>();
            while (dr.Read())
            {
                ToReturn.Add(ExtractTelemetrySnapshotFromSqlDataReader(dr));
            }
            sqlcon.Close();
            return ToReturn.ToArray();
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
    
    
    
        #region "Sql Data Reader Extraction"

        public TelemetrySnapshot ExtractTelemetrySnapshotFromSqlDataReader(SqlDataReader dr, string prefix = "")
        {
            TelemetrySnapshot ToReturn = new TelemetrySnapshot();

            //Id
            try
            {
                ToReturn.Id = dr.GetGuid(dr.GetOrdinal(prefix + "Id"));
            }
            catch
            {

            }

            //FromSession
            try
            {
                ToReturn.FromSession = dr.GetGuid(dr.GetOrdinal(prefix + "FromSession"));
            }
            catch
            {

            }

            //Acceleration
            try
            {
                ToReturn.AccelerationX = dr.GetFloat(dr.GetOrdinal(prefix + "AccelerationX"));
            }
            catch
            {

            }
            try
            {
                ToReturn.AccelerationY = dr.GetFloat(dr.GetOrdinal(prefix + "AccelerationY"));
            }
            catch
            {

            }
            try
            {
                ToReturn.AccelerationZ = dr.GetFloat(dr.GetOrdinal(prefix + "AccelerationZ"));
            }
            catch
            {

            }

            //Gyro
            try
            {
                ToReturn.GyroscopeX = dr.GetFloat(dr.GetOrdinal(prefix + "GyroscopeX"));
            }
            catch
            {

            }
            try
            {
                ToReturn.GyroscopeY = dr.GetFloat(dr.GetOrdinal(prefix + "GyroscopeY"));
            }
            catch
            {

            }
            try
            {
                ToReturn.GyroscopeZ = dr.GetFloat(dr.GetOrdinal(prefix + "GyroscopeZ"));
            }
            catch
            {

            }

            //Magneto
            try
            {
                ToReturn.MagnetoX = dr.GetFloat(dr.GetOrdinal(prefix + "MagnetoX"));
            }
            catch
            {

            }
            try
            {
                ToReturn.MagnetoY = dr.GetFloat(dr.GetOrdinal(prefix + "MagnetoY"));
            }
            catch
            {

            }
            try
            {
                ToReturn.MagnetoZ = dr.GetFloat(dr.GetOrdinal(prefix + "MagnetoZ"));
            }
            catch
            {

            }

            //Lat + Long
            try
            {
                ToReturn.Latitude = dr.GetFloat(dr.GetOrdinal(prefix + "Latitude"));
            }
            catch
            {

            }
            try
            {
                ToReturn.Longitude = dr.GetFloat(dr.GetOrdinal(prefix + "Longitude"));
            }
            catch
            {

            }

            //CapturedAtutc
            try
            {
                ToReturn.CapturedAtUtc = dr.GetDateTime(dr.GetOrdinal(prefix + "CapturedAtUtc"));
            }
            catch
            {

            }

            //Orientaiton
            try
            {
                ToReturn.OrientationX = dr.GetFloat(dr.GetOrdinal(prefix + "OrientationX"));
            }
            catch
            {

            }
            try
            {
                ToReturn.OrientationY = dr.GetFloat(dr.GetOrdinal(prefix + "OrientationY"));
            }
            catch
            {

            }
            try
            {
                ToReturn.OrientationZ = dr.GetFloat(dr.GetOrdinal(prefix + "OrientationZ"));
            }
            catch
            {

            }

            //GPS Accuracy
            try
            {
                ToReturn.GpsAccuracy = dr.GetFloat(dr.GetOrdinal(prefix + "GpsAccuracy"));
            }
            catch
            {

            }

            //Speed meters per second
            try
            {
                ToReturn.SpeedMetersPerSecond = dr.GetFloat(dr.GetOrdinal(prefix = "SpeedMetersPerSecond"));
            }
            catch
            {

            }

            return ToReturn;
        }

        #endregion
    
    
    
    }
}