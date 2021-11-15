using System;
using TimHanewich.TelemetryFeed;
using TimHanewich.Toolkit;
using TimHanewich.Toolkit.Geo;
using System.Collections.Generic;
using System.Linq;

namespace TimHanewich.TelemetryFeed.Analysis
{
    public class AnalysisEngine
    {
        //LAST RECEIVED 
        private TelemetrySnapshot LastReceivedTelemetrySnapshot;

        //Earliest received and latest received (for total time)
        private TelemetrySnapshot OldestReceived;
        private TelemetrySnapshot YoungestReceived;

        //for stops
        private TelemetrySnapshot StationaryFirstNoticed;
        private RiderStatus _Status;
        private List<StationaryStop> _Stops = new List<StationaryStop>();

        //Top speed
        private float? _TopSpeedMph;
        private Guid? _TopSpeedDetectedAt; //Guid of telemetry snapshot where the top speed was detected

        public void Feed(TelemetrySnapshot ts)
        {
            //First seen? Last seen?
            if (OldestReceived != null)
            {
                if (ts.CapturedAtUtc < OldestReceived.CapturedAtUtc)
                {
                    OldestReceived = ts;
                }
            }
            else
            {
                OldestReceived = ts;
            }
            if (YoungestReceived != null)
            {
                if (ts.CapturedAtUtc > YoungestReceived.CapturedAtUtc)
                {
                    YoungestReceived = ts;
                }
            }
            else
            {
                YoungestReceived = ts;
            }

            //Check for top speed
            if (CurrentSpeedMph.HasValue)
            {
                float speed = CurrentSpeedMph.Value;
                if (_TopSpeedMph.HasValue)
                {
                    if (speed > _TopSpeedMph.Value)
                    {
                        _TopSpeedMph = speed;
                        _TopSpeedDetectedAt = ts.Id;
                    }
                }
                else
                {
                    _TopSpeedMph = speed;
                    _TopSpeedDetectedAt = ts.Id;
                }
            }
            

            //Check for stop
            if (CurrentSpeedMph.HasValue)
            {
                if (CurrentSpeedMph.Value < 2) //SPEEDS BELOW THIS INDICATE a 'stop'
                {
                    if (_Status == RiderStatus.Moving)
                    {
                        _Status = RiderStatus.Stationary; //Mark as stationary
                        StationaryFirstNoticed = ts; //Mark the first time we are seeing it is stationary. 
                    }
                }
                else //We are over the minimum speed and are now moving
                {
                    if (_Status == RiderStatus.Stationary) //If it was previously marked as stationary
                    {
                        TimeSpan TimeSinceStationaryBegan = ts.CapturedAtUtc - StationaryFirstNoticed.CapturedAtUtc;
                        StationaryStop ss = new StationaryStop();
                        ss.Beginning = StationaryFirstNoticed.Id;
                        ss.End = ts.Id;
                        if (StationaryFirstNoticed.Latitude.HasValue)
                        {
                            ss.Latitude = StationaryFirstNoticed.Latitude.Value;
                        }
                        if (StationaryFirstNoticed.Longitude.HasValue)
                        {
                            ss.Longitude = StationaryFirstNoticed.Longitude.Value;
                        }
                        ss.BeganAtUtc = StationaryFirstNoticed.CapturedAtUtc;
                        ss.EndedAtUtc = ts.CapturedAtUtc;
                        _Stops.Add(ss);
                        
                        //Flip and clear
                        _Status = RiderStatus.Moving;
                        StationaryFirstNoticed = null;
                    }
                }
            }


            //SET LAST RECEIVED!
            LastReceivedTelemetrySnapshot = ts;
            
        }

        public float? CurrentSpeedMph
        {
            get
            {
                if (LastReceivedTelemetrySnapshot != null)
                {
                    if (LastReceivedTelemetrySnapshot.SpeedMetersPerSecond.HasValue)
                    {
                        return LastReceivedTelemetrySnapshot.SpeedMetersPerSecond.Value * 2.23694f;
                    }
                }
                return null; //return null if the above didn't work.
            }
        }

        public StationaryStop[] Stops
        {
            get
            {
                return _Stops.ToArray();
            }
        }
    
        public float? TopSpeedMph
        {
            get
            {
                return _TopSpeedMph;
            }
        }

        public Guid? TopSpeedDetectedAt
        {
            get
            {
                return _TopSpeedDetectedAt;
            }
        }
    
        #region "Riding timing statistics"

        public TimeSpan TotalTime
        {
            get
            {
                if (OldestReceived != null && YoungestReceived != null)
                {
                    TimeSpan ts = YoungestReceived.CapturedAtUtc - OldestReceived.CapturedAtUtc;
                    return ts;
                }
                else
                {
                    return new TimeSpan(0, 0, 0);
                }
            }
        }
    
        public TimeSpan TotalTimeStationary
        {
            get
            {
                TimeSpan ToAddTo = new TimeSpan(0, 0, 0);
                foreach (StationaryStop ss in Stops)
                {
                    TimeSpan StopDuration = ss.Duration();
                    ToAddTo = ToAddTo + StopDuration;
                }
                return ToAddTo;
            }
        }

        public TimeSpan TotalTimeMoving
        {
            get
            {
                TimeSpan tt = TotalTime;
                TimeSpan ts = TotalTimeStationary;
                return tt - ts;
            }
        }
    
        public float PercentTimeStationary
        {
            get
            {
                TimeSpan total = TotalTime;
                TimeSpan stationary = TotalTimeStationary;
                float ToReturn = Convert.ToSingle(stationary.TotalSeconds) / Convert.ToSingle(total.TotalSeconds);
                return ToReturn;
            }
        }

        public float PercentTimeMoving
        {
            get
            {
                float PercentStationary = PercentTimeStationary;
                return 1f - PercentStationary;
            }
        }

        #endregion
    
    }
}