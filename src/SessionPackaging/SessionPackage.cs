using System;
using System.IO;
using TimHanewich.TelemetryFeed;
using System.IO.Compression;
using Newtonsoft.Json;

namespace TimHanewich.TelemetryFeed.SessionPackaging
{
    public class SessionPackage
    {
        //Package serializer version number
        public const int Version = 1;

        public Session Session {get; set;}
        public TelemetrySnapshot LeftLeanCalibration {get; set;}
        public TelemetrySnapshot RightLeanCalibration {get; set;}
        public TelemetrySnapshot[] TelemetrySnapshots {get; set;}

        public byte[] Pack()
        {
            MemoryStream ToReturn = new MemoryStream();
            ZipArchive za = new ZipArchive(ToReturn, ZipArchiveMode.Create, true);

            //drop the version number
            ZipArchiveEntry VersionZAE = za.CreateEntry("version.txt");
            Stream twt_Version = VersionZAE.Open();
            StreamWriter sw_Version = new StreamWriter(twt_Version);
            sw_Version.Write(Version.ToString());
            sw_Version.Dispose();
            twt_Version.Dispose();

            //Create the session stream (JSON)
            if (Session != null)
            {
                ZipArchiveEntry SessionZAE = za.CreateEntry("Session.json");
                Stream twt_Session = SessionZAE.Open();
                StreamWriter sw = new StreamWriter(twt_Session);
                sw.Write(JsonConvert.SerializeObject(Session));
                sw.Dispose();
                twt_Session.Dispose();
            }
            


            //Create the LeftLeanCalibration Stream
            if (LeftLeanCalibration != null)
            {
                MemoryStream ms_LeftCalibration = new MemoryStream(LeftLeanCalibration.ToBytes());
                ZipArchiveEntry LeftLean = za.CreateEntry("LeftLeanCalibration");
                Stream twt_LL = LeftLean.Open();
                ms_LeftCalibration.CopyTo(twt_LL);
                twt_LL.Dispose();
                ms_LeftCalibration.Dispose();
            }
            

            //Create the RightLeanCalibration Straem
            if (RightLeanCalibration != null)
            {
                MemoryStream ms_RightCalibration = new MemoryStream(RightLeanCalibration.ToBytes());
                ZipArchiveEntry RightLean = za.CreateEntry("RightLeanCalibration");
                Stream twt_RL = RightLean.Open();
                ms_RightCalibration.CopyTo(twt_RL);
                twt_RL.Dispose();
                ms_RightCalibration.Dispose();
            }
            

            //Add all of the telemetry snapshots
            if (TelemetrySnapshots != null)
            {
                if (TelemetrySnapshots.Length > 0)
                {
                    MemoryStream ms_TelemetrySnapshots = new MemoryStream(TelemetrySnapshots.ToBytes());
                    ZipArchiveEntry SnapshotsZAE = za.CreateEntry("TelemetrySnapshots");
                    Stream twt_Snapshots = SnapshotsZAE.Open();
                    ms_TelemetrySnapshots.CopyTo(twt_Snapshots);
                    twt_Snapshots.Dispose();
                    ms_TelemetrySnapshots.Dispose();
                }
            }
            

            //Close the zip archive (but it will still be kept open because the true parameter when making it)
            za.Dispose();

            ToReturn.Position = 0;
            return ToReturn.ToArray();
        }
    
        public static SessionPackage Unpack(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            return Unpack(ms);
        }    

        public static SessionPackage Unpack(MemoryStream ms)
        {
            SessionPackage ToReturn = new SessionPackage();
            ZipArchive za = new ZipArchive(ms, ZipArchiveMode.Read);
            
            //Check the version
            ZipArchiveEntry VersionZAE = za.GetEntry("version.txt");
            if (VersionZAE == null)
            {
                throw new Exception("Version number not found in package.");
            }
            StreamReader srv = new StreamReader(VersionZAE.Open());
            string content = srv.ReadToEnd();
            int PackageVersionNo = 0;
            try
            {
                PackageVersionNo = Convert.ToInt32(content);
            }
            catch
            {
                throw new Exception("Did not recognize version '" + content + "' as a valid version number.");
            }
            if (PackageVersionNo != Version)
            {
                throw new Exception("Package is an old version type. Package version: " + PackageVersionNo.ToString() + ". Current version: " + Version.ToString());
            }

            //Get the session
            ZipArchiveEntry SessionZAE = za.GetEntry("Session.json");
            if (SessionZAE != null)
            {
                Stream SessionStream = SessionZAE.Open();
                StreamReader sr = new StreamReader(SessionStream);
                string SessionJson = sr.ReadToEnd();
                ToReturn.Session = JsonConvert.DeserializeObject<Session>(SessionJson);
            }
            

            //Get left lean
            ZipArchiveEntry LeftLeanZAE = za.GetEntry("LeftLeanCalibration");
            if (LeftLeanZAE != null)
            {
                Stream LeftLeanStream = LeftLeanZAE.Open();
                MemoryStream ms_LeftLean = new MemoryStream();
                LeftLeanStream.CopyTo(ms_LeftLean);
                ms_LeftLean.Position = 0;
                byte[] LLBytes = ms_LeftLean.ToArray();
                TelemetrySnapshot ts_LeftLean = TelemetrySnapshot.FromBytes(LLBytes);
                ToReturn.LeftLeanCalibration = ts_LeftLean;
            }
            

            //Get right lean
            ZipArchiveEntry RightLeanZAE = za.GetEntry("RightLeanCalibration");
            if (RightLeanZAE != null)
            {
                Stream RightLeanStream = RightLeanZAE.Open();
                MemoryStream ms_RightLean = new MemoryStream();
                RightLeanStream.CopyTo(ms_RightLean);
                ms_RightLean.Position = 0;
                byte[] RLBytes = ms_RightLean.ToArray();
                TelemetrySnapshot ts_RightLean = TelemetrySnapshot.FromBytes(RLBytes);
                ToReturn.RightLeanCalibration = ts_RightLean;
            }
            

            //Get the telemetry snapshots
            ZipArchiveEntry TelemetrySnapshotsZAE = za.GetEntry("TelemetrySnapshots");
            if (TelemetrySnapshotsZAE != null)
            {
                Stream TelemetrySnapshotsStream = TelemetrySnapshotsZAE.Open();
                MemoryStream ms_TelemetrySnapshots = new MemoryStream();
                TelemetrySnapshotsStream.CopyTo(ms_TelemetrySnapshots);
                ms_TelemetrySnapshots.Position = 0;
                byte[] TSBytes = ms_TelemetrySnapshots.ToArray();
                TelemetrySnapshot[] snapshots = TelemetrySnapshot.ArrayFromBytes(TSBytes);
                ToReturn.TelemetrySnapshots = snapshots;
            }
            

            return ToReturn;

        }

    }
}