using System;
using System.Collections.Generic;
using TimHanewich.Toolkit;

namespace TimHanewich.TelemetryFeed
{
    public static class TelemetryFeedToolkit
    {
        public static byte[] ToBytes(this TelemetrySnapshot[] snapshots)
        {
            List<byte> ToReturn = new List<byte>();

            foreach (TelemetrySnapshot ts in snapshots)
            {
                ToReturn.AddRange(ts.ToBytes());
            }

            return ToReturn.ToArray();
        }

        public static TelemetrySnapshot[] ToTelemetrySnapshots(byte[] bytes)
        {
            int SingleTelemetrySnapshotLength = 100; //The length (in bytes) of a single telemetry snapshot
            float NumberOfSnapshotsF = Convert.ToSingle(bytes.Length) / Convert.ToSingle(SingleTelemetrySnapshotLength);

            //Get number of snapshots
            int NumberOfSnapshots = 0;
            try
            {
                NumberOfSnapshots = Convert.ToInt32(NumberOfSnapshotsF);
            }
            catch
            {
                throw new Exception("The number of bytes supplied doesn't make an even number of Telemetry Snapshots");
            }

            //Get each
            ByteArrayManager bam = new ByteArrayManager(bytes);
            List<TelemetrySnapshot> ToReturn = new List<TelemetrySnapshot>();
            for (int t = 0; t < NumberOfSnapshots; t++)
            {
                byte[] thesebytes = bam.NextBytes(SingleTelemetrySnapshotLength);
                TelemetrySnapshot ts = TelemetrySnapshot.FromBytes(thesebytes);
                ToReturn.Add(ts);
            }

            return ToReturn.ToArray();
        }
    }
}