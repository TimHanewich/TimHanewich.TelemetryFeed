using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using System.Data.Sql;
using TimHanewich.SqlHelper;

namespace TimHanewich.TelemetryFeed.Sql
{
    public static class CoreSqlExtensions
    {
        public static async Task UploadRegisteredUserAsync(this TelemetryFeedSqlClient cc, RegisteredUser user)
        {
            InsertHelper ih = new InsertHelper("RegisteredUser");
            ih.Add("Id", user.Id.ToString(), true);
            ih.Add("Username", user.Username, true);
            ih.Add("Password", user.Password, true);
            await cc.ExecuteNonQueryAsync(ih.ToString());
        }

        public static async Task UploadSessionAsync(this TelemetryFeedSqlClient cc, Session s)
        {
            InsertHelper ih = new InsertHelper("Session");
            ih.Add("Id", s.Id.ToString(), true);
            ih.Add("Owner", s.Owner.ToString(), true);
            await cc.ExecuteNonQueryAsync(ih.ToString());
        }

        public static async Task UploadTelemetrySnapshotAsync(this TelemetryFeedSqlClient cc, TelemetrySnapshot ts)
        {
            InsertHelper ih = new InsertHelper("TelemetrySnapshot");

            ih.Add("Id", ts.Id.ToString(), true);
            ih.Add("FromSession", ts.FromSession.ToString(), true);
            ih.Add("CapturedAtUtc", TimHanewich.SqlHelper.SqlToolkit.ToSqlDateTimeString(ts.CapturedAtUtc), true);
            
            //Acceleration
            if (ts.AccelerationX.HasValue)
            {
                ih.Add("AccelerationX", ts.AccelerationX.Value.ToString());
            }
            if (ts.AccelerationY.HasValue)
            {
                ih.Add("AccelerationY", ts.AccelerationY.Value.ToString());
            }
            if (ts.AccelerationZ.HasValue)
            {
                ih.Add("AccelerationZ", ts.AccelerationZ.Value.ToString());
            }

            //Gyroscope
            if (ts.GyroscopeX.HasValue)
            {
                ih.Add("GyroscopeX", ts.GyroscopeX.Value.ToString());
            }
            if (ts.GyroscopeY.HasValue)
            {
                ih.Add("GyroscopeY", ts.GyroscopeY.Value.ToString());
            }
            if (ts.GyroscopeZ.HasValue)
            {
                ih.Add("GyroscopeZ", ts.GyroscopeZ.Value.ToString());
            }

            //Magneto
            if (ts.MagnetoX.HasValue)
            {
                ih.Add("MagnetoX", ts.MagnetoX.Value.ToString());
            }
            if (ts.MagnetoY.HasValue)
            {
                ih.Add("MagnetoY", ts.MagnetoY.Value.ToString());
            }
            if (ts.MagnetoZ.HasValue)
            {
                ih.Add("MagnetoZ", ts.MagnetoZ.Value.ToString());
            }

            //Orientation
            if (ts.OrientationX.HasValue)
            {
                ih.Add("OrientationX", ts.OrientationX.ToString());
            }
            if (ts.OrientationY.HasValue)
            {
                ih.Add("OrientationY", ts.OrientationY.ToString());
            }
            if (ts.OrientationZ.HasValue)
            {
                ih.Add("OrientationZ", ts.OrientationZ.ToString());
            }

            //Lat and long
            if (ts.Latitude.HasValue)
            {
                ih.Add("Latitude", ts.Latitude.Value.ToString());
            }
            if (ts.Longitude.HasValue)
            {
                ih.Add("Longitude", ts.Longitude.Value.ToString());
            }

            await cc.ExecuteNonQueryAsync(ih.ToString());
        }
    }
}