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
        public float BeginningSpeedMetersPerSecond {get; set;}
        public float EndingSpeedMetersPerSecond {get; set;}

        public TimeSpan Duration
        {
            get
            {
                return EndingUtc - BeginningUtc;
            }
        }

        public AccelerationStatus VelocityChangeType
        {
            get
            {
                if (EndingSpeedMetersPerSecond > BeginningSpeedMetersPerSecond)
                {
                    return AccelerationStatus.Accelerating;
                }
                else if (EndingSpeedMetersPerSecond < BeginningSpeedMetersPerSecond)
                {
                    return AccelerationStatus.Decelerating;
                }
                else //This should never happen because a velocity change where the ending velocity is the same as the beginning velocity is invalid.
                {
                    return AccelerationStatus.MaintainingSpeed;
                }
            }
        }
    }
}