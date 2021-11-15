using System;
using System.IO;
using TimHanewich.TelemetryFeed;
using TimHanewich.TelemetryFeed.Analysis;

namespace TimHanewich.TelemetryFeed.Analysis
{
    public class VelocityChange
    {
        //Date/Time logging for when started and stopped
        public DateTime BeginningUtc {get; set;}
        public Guid BeginningSnapshot {get; set;}
        public DateTime EndingUtc {get; set;}
        public Guid EndingSnapshot {get; set;}

        public float VelocityChangeMetersPerSecond {get; set;}
    }
}