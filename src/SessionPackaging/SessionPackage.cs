using System;
using System.IO;
using TimHanewich.TelemetryFeed;

namespace TimHanewich.TelemetryFeed.SessionPackaging
{
    public class SessionPackage
    {
        public Session Session {get; set;}
        public TelemetrySnapshot LeftLeanCalibration {get; set;}
        public TelemetrySnapshot RightLeanCalibration {get; set;}
        public TelemetrySnapshot[] TelemetrySnapshots {get; set;}
    }
}