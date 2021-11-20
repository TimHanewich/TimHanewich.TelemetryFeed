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
        //To SQL query

        public static string ToSqlInsert(this RegisteredUser user)
        {
            InsertHelper ih = new InsertHelper("RegisteredUser");
            ih.Add("Id", user.Id.ToString(), true);
            ih.Add("Username", user.Username, true);
            ih.Add("Password", user.Password, true);
            return ih.ToString();
        }

        public static string ToSqlInsert(this Session s)
        {
            InsertHelper ih = new InsertHelper("Session");
            ih.Add("Id", s.Id.ToString(), true);
            ih.Add("Owner", s.Owner.ToString(), true);
            if (s.Title != null)
            {
                if (s.Title != "")
                {
                    ih.Add("Title", s.Title, true);
                }
            }
            ih.Add("CreatedAtUtc", SqlToolkit.ToSqlDateTimeString(s.CreatedAtUtc), true);
            if (s.RightLeanCalibration.HasValue)
            {
                ih.Add("RightLeanCalibration", s.RightLeanCalibration.Value.ToString(), true);
            }
            if (s.LeftLeanCalibration.HasValue)
            {
                ih.Add("LeftLeanCalibration", s.LeftLeanCalibration.Value.ToString(), true);
            }
            if (s.IntendedDestinationLatitude.HasValue)
            {
                ih.Add("IntendedDestinationLatitude", s.IntendedDestinationLatitude.Value.ToString());
            }
            if (s.IntendedDestinationLongitude.HasValue)
            {
                ih.Add("IntendedDestinationLongitude", s.IntendedDestinationLongitude.Value.ToString());
            }
            if (s.ClientVersionCode.HasValue)
            {
                ih.Add("ClientVersionCode", s.ClientVersionCode.Value.ToString());
            }
            
            return ih.ToString();
        }

        public static string ToSqlInsert(this TelemetrySnapshot ts)
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

            //Lat and long and GPS location
            if (ts.Latitude.HasValue)
            {
                ih.Add("Latitude", ts.Latitude.Value.ToString());
            }
            if (ts.Longitude.HasValue)
            {
                ih.Add("Longitude", ts.Longitude.Value.ToString());
            }
            if (ts.GpsAccuracy.HasValue)
            {
                if (ts.GpsAccuracy.Value != float.NaN)
                {
                    ih.Add("GpsAccuracy", ts.GpsAccuracy.Value.ToString());
                }
            }

            //SpeedMetersPerSecond
            if (ts.SpeedMetersPerSecond.HasValue)
            {
                ih.Add("SpeedMetersPerSecond", ts.SpeedMetersPerSecond.Value.ToString());
            }


            return ih.ToString();
        }


        //DOWNLOADS
        public static string DownloadSessions(Guid owner_id)
        {
            string cmd = "select Id,Owner,Title,CreatedAtUtc,RightLeanCalibration,LeftLeanCalibration, ClientVersionCode from Session where Owner = '" + owner_id.ToString() + "' order by CreatedAtUtc desc";
            return cmd;
        }

        public static string DownloadSession(Guid id)
        {
            string cmd = "select Id,Owner,Title,CreatedAtUtc,RightLeanCalibration,LeftLeanCalibration, ClientVersionCode from Session where Id = '" + id.ToString() + "'";
            return cmd;
        }

        public static string DownloadRegisteredUser(string username)
        {
            string cmd = "select Id, Username, Password from RegisteredUser where Username = '" + username + "'";
            return cmd;
        }

        public static string DownloadRecentSessions(int top = 5)
        {
            string cmd = "select top " + top.ToString() + " Id, Owner, Title, CreatedAtUtc, RightLeanCalibration, LeftLeanCalibration, IntendedDestinationLatitude, IntendedDestinationLongitude from Session order by CreatedAtUtc desc";
            return cmd;
        }

        public static string DownloadRegisteredUser(Guid id)
        {
            string cmd = "select Id, Username, Password from RegisteredUser where Id = '" + id.ToString() + "'";
            return cmd;
        }
        
        //Arranged from newest to oldest
        public static string DownloadTelemetrySnapshots(Guid from_session, int top = 1)
        {
            string cmd = "select top " + top.ToString() + " Id, FromSession, CapturedAtUtc, AccelerationX, AccelerationY, AccelerationZ, GyroscopeX, GyroscopeY, GyroscopeZ, MagnetoX, MagnetoY, MagnetoZ, Latitude, Longitude, GpsAccuracy, OrientationX, OrientationY, OrientationZ, SpeedMetersPerSecond from TelemetrySnapshot where FromSession = '" + from_session.ToString() + "' order by CapturedAtUtc desc";
            return cmd;
        }

        public static string DownloadTelemetrySnapshot(Guid id)
        {
            string cmd = "select Id, FromSession, CapturedAtUtc, AccelerationX, AccelerationY, AccelerationZ, GyroscopeX, GyroscopeY, GyroscopeZ, MagnetoX, MagnetoY, MagnetoZ, Latitude, Longitude, GpsAccuracy, OrientationX, OrientationY, OrientationZ, SpeedMetersPerSecond from TelemetrySnapshot where Id = '" + id.ToString() + "'";
            return cmd;
        }
    }
}