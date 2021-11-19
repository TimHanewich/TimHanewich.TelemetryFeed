using System;
using System.IO;
using TimHanewich.TelemetryFeed;
using System.IO.Compression;
using Newtonsoft.Json;

namespace TimHanewich.TelemetryFeed.SessionPackaging
{
    public class SessionPackage
    {
        public Session Session {get; set;}
        public TelemetrySnapshot LeftLeanCalibration {get; set;}
        public TelemetrySnapshot RightLeanCalibration {get; set;}
        public TelemetrySnapshot[] TelemetrySnapshots {get; set;}

        public byte[] Package()
        {
            MemoryStream ToReturn = new MemoryStream();
            ZipArchive za = new ZipArchive(ToReturn, ZipArchiveMode.Create, true);

            //Create the session stream (JSON)
            ZipArchiveEntry SessionZAE = za.CreateEntry("Session");
            Stream twt_Session = SessionZAE.Open();
            StreamWriter sw = new StreamWriter(twt_Session);
            sw.Write(JsonConvert.SerializeObject(Session));
            sw.Dispose();
            twt_Session.Dispose();


            //Create the LeftLeanCalibration Stream
            MemoryStream ms_LeftCalibration = new MemoryStream(LeftLeanCalibration.ToBytes());
            ZipArchiveEntry LeftLean = za.CreateEntry("LeftLeanCalibration");
            Stream twt_LL = LeftLean.Open();
            ms_LeftCalibration.CopyTo(twt_LL);
            twt_LL.Dispose();
            ms_LeftCalibration.Dispose();

            //Create the RightLeanCalibration Straem
            MemoryStream ms_RightCalibration = new MemoryStream(RightLeanCalibration.ToBytes());
            ZipArchiveEntry RightLean = za.CreateEntry("RightLeanCalibration");
            Stream twt_RL = RightLean.Open();
            ms_RightCalibration.CopyTo(twt_RL);
            twt_RL.Dispose();
            ms_RightCalibration.Dispose();

            //Add all of the telemetry snapshots
            MemoryStream ms_TelemetrySnapshots = new MemoryStream(TelemetrySnapshots.ToBytes());
            ZipArchiveEntry SnapshotsZAE = za.CreateEntry("TelemetrySnapshots");
            Stream twt_Snapshots = SnapshotsZAE.Open();
            ms_TelemetrySnapshots.CopyTo(twt_Snapshots);
            twt_Snapshots.Dispose();
            ms_TelemetrySnapshots.Dispose();

            //Close the zip archive (but it will still be kept open because the true parameter when making it)
            za.Dispose();

            ToReturn.Position = 0;
            return ToReturn.ToArray();
        }
    }
}