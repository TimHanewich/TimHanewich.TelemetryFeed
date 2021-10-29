using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimHanewich.TelemetryFeed.SessionManagement
{
    // public class SessionManager
    // {
    //     private CloudClient ToUseCloudClient;
    //     private List<TelemetrySnapshotUploadStatus> TelemetrySnapshots;

    //     //In-situ uploading
    //     public bool InSituUploadEnabled {get; set;}
    //     public TimeSpan InSituUploadFrequency {get; set;}
    //     private TelemetrySnapshot LastUploadedTelemetrySnapshot;
        
    //     public SessionManager(CloudClient authenticated_cloud_client)
    //     {
    //         ToUseCloudClient = authenticated_cloud_client;
    //         TelemetrySnapshots = new List<TelemetrySnapshotUploadStatus>();
    //         InSituUploadFrequency = new TimeSpan(0, 0, 10);
    //         InSituUploadEnabled = false;
    //     }

    //     public async void IngestTelemetrySnapshot(TelemetrySnapshot ts)
    //     {

    //         //Upload?
    //         bool WasUploaded = false;
    //         if (InSituUploadEnabled)
    //         {
    //             //Is it time to upload?
    //             bool TimeToUpload = false;
    //             if (LastUploadedTelemetrySnapshot == null)
    //             {
    //                 TimeToUpload = true;
    //             }
    //             else
    //             {
    //                 TimeSpan TimeSinceLastUpload = DateTime.UtcNow - LastUploadedTelemetrySnapshot.CapturedAtUtc;
    //                 if (TimeSinceLastUpload > InSituUploadFrequency)
    //                 {
    //                     TimeToUpload = true;
    //                 }
    //             }
            
    //             //Upload if it is time to upload
    //             if (TimeToUpload)
    //             {
    //                 await ToUseCloudClient.UploadTelemetrySnapshotAsync(ts);
    //                 LastUploadedTelemetrySnapshot = ts;
    //                 WasUploaded = true;
    //             }
    //         }

    //         //Add it
    //         TelemetrySnapshotUploadStatus status = new TelemetrySnapshotUploadStatus();
    //         status.Snapshot = ts;
    //         status.Uploaded = WasUploaded;
    //         TelemetrySnapshots.Add(status);
    //     }

    //     public async Task UploadUnUploadedTelemetrySnapshotsAsync()
    //     {
    //         for (int t = 0; t < TelemetrySnapshots.Count - 1; t++)
    //         {
    //             if (TelemetrySnapshots[t].Uploaded == false)
    //             {
    //                 await ToUseCloudClient.UploadTelemetrySnapshotAsync(TelemetrySnapshots[t].Snapshot);
    //                 TelemetrySnapshots[t].Uploaded = true;
    //             }
    //         }
    //     }

    //     public int CountTelemetrySnapshots()
    //     {
    //         return TelemetrySnapshots.Count;
    //     }

    //     public int CountUploadedTelemetrySnapshots()
    //     {
    //         int val = 0;
    //         foreach (TelemetrySnapshotUploadStatus kvp in TelemetrySnapshots)
    //         {
    //             if (kvp.Uploaded == true)
    //             {
    //                 val = val + 1;
    //             }
    //         }
    //         return val;
    //     }

    //     public int CountUnUploadedTelemetrySnapshots()
    //     {
    //         int val = 0;
    //         foreach (TelemetrySnapshotUploadStatus kvp in TelemetrySnapshots)
    //         {
    //             if (kvp.Uploaded == false)
    //             {
    //                 val = val + 1;
    //             }
    //         }
    //         return val;
    //     }

    // }
}