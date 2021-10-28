using System;

namespace TimHanewich.TelemetryFeed.SessionManagement
{
    public class TelemetrySnapshotUploadStatus
    {
        public TelemetrySnapshot Snapshot {get; set;}
        public bool Uploaded {get; set;}
    }
}