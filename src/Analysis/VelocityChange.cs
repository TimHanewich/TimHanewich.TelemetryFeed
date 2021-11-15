using System;
using System.IO;
using TimHanewich.TelemetryFeed;
using TimHanewich.TelemetryFeed.Analysis;
using System.Collections.Generic;

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

        //The average change in velocity in meters per second per second
        public float AverageVelocityChangeMPS2 
        {
            get
            {
                float SpeedDiff = EndingSpeedMetersPerSecond - BeginningSpeedMetersPerSecond;
                float ToReturn = SpeedDiff / Convert.ToSingle(Duration.TotalSeconds);
                return ToReturn;
            }
        }
    
    
        #region "Toolkit"

        //Sorts from highest velocity to lowest velocity change
        public static VelocityChange[] Sort(VelocityChange[] changes)
        {
            if (changes == null)
            {
                return new VelocityChange[]{};
            }
            else if (changes.Length == 0)
            {
                return new VelocityChange[]{};
            }
            else if (changes.Length == 1)
            {
                return new VelocityChange[]{changes[0]};
            }

            List<VelocityChange> ToPullFrom = new List<VelocityChange>();
            ToPullFrom.AddRange(changes);
            List<VelocityChange> ToReturn = new List<VelocityChange>();
            while (ToPullFrom.Count > 0)
            {
                VelocityChange winner = ToPullFrom[0];
                foreach (VelocityChange vc in ToPullFrom)
                {
                    if (vc.AverageVelocityChangeMPS2 > winner.AverageVelocityChangeMPS2)
                    {
                        winner = vc;
                    }
                }

                ToReturn.Add(winner);
                ToPullFrom.Remove(winner);
            }

            return ToReturn.ToArray();
        }


        #endregion
    
    }
}