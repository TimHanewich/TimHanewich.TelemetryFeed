using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimHanewich.TelemetryFeed.SessionManagement
{
    public class SessionManager
    {
        private CloudClient ToUseCloudClient;
        private List<KeyValuePair<TelemetrySnapshot, bool>> TelemetrySnapshots; //bool represents "IsUploaded"

        //In-situ uploading
        public bool InSituUploadingEnabled {get; set;}
        public TimeSpan InSituUploadFrequency {get; set;}
        private TelemetrySnapshot LastUploadedTelemetrySnapshot;
        
        public SessionManager(CloudClient authenticated_cloud_client)
        {
            ToUseCloudClient = authenticated_cloud_client;
            TelemetrySnapshots = new List<KeyValuePair<TelemetrySnapshot, bool>>();
            InSituUploadFrequency = new TimeSpan(0, 0, 10);
            InSituUploadingEnabled = false;
        }

        public async void IngestTelemetrySnapshot(TelemetrySnapshot ts)
        {

            //Upload?
            bool WasUploaded = false;
            if (InSituUploadingEnabled)
            {
                //Is it time to upload?
                bool TimeToUpload = false;
                if (LastUploadedTelemetrySnapshot == null)
                {
                    TimeToUpload = true;
                }
                else
                {
                    TimeSpan TimeSinceLastUpload = DateTime.UtcNow - LastUploadedTelemetrySnapshot.CapturedAtUtc;
                    if (TimeSinceLastUpload > InSituUploadFrequency)
                    {
                        TimeToUpload = true;
                    }
                }
            
                //Upload if it is time to upload
                if (TimeToUpload)
                {
                    await ToUseCloudClient.UploadTelemetrySnapshotAsync(ts);
                    LastUploadedTelemetrySnapshot = ts;
                    WasUploaded = true;
                }
            }

            //Add it
            TelemetrySnapshots.Add(new KeyValuePair<TelemetrySnapshot, bool>(ts, WasUploaded));
        }

        public int CountTelemetrySnapshots()
        {
            return TelemetrySnapshots.Count;
        }

        public int CountUploadedTelemetrySnapshots()
        {
            int val = 0;
            foreach (KeyValuePair<TelemetrySnapshot, bool> kvp in TelemetrySnapshots)
            {
                if (kvp.Value == true)
                {
                    val = val + 1;
                }
            }
            return val;
        }

        public int CountUnUploadedTelemetrySnapshots()
        {
            int val = 0;
            foreach (KeyValuePair<TelemetrySnapshot, bool> kvp in TelemetrySnapshots)
            {
                if (kvp.Value == false)
                {
                    val = val + 1;
                }
            }
            return val;
        }

    }
}