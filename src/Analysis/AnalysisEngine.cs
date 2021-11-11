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
        private TimeSpan BufferGuidelines;
        private List<TelemetrySnapshot> LastReceivedTelemetrySnapshots;

        //for stops
        private TelemetrySnapshot ZeroDistanceCoveredFirstNoticed;
        private RiderStatus _Status;
        private List<StationaryStop> _Stops = new List<StationaryStop>();

        //Top speed
        private float _TopSpeedMph;
        private Guid _TopSpeedDetectedAt; //Guid of telemetry snapshot where the top speed was detected

        public AnalysisEngine()
        {
            LastReceivedTelemetrySnapshots = new List<TelemetrySnapshot>();
            BufferGuidelines = new TimeSpan(0, 0, 7);
        }

        public void Feed(TelemetrySnapshot ts)
        {
            //Add it to the buffer
            AddSnapshotToBuffer(ts);
        }

        private void AddSnapshotToBuffer(TelemetrySnapshot ts)
        {
            if (LastReceivedTelemetrySnapshots == null)
            {
                LastReceivedTelemetrySnapshots = new List<TelemetrySnapshot>();
            }
            LastReceivedTelemetrySnapshots.Add(ts);

            //Find the most recent one
            TelemetrySnapshot most_recent = LastReceivedTelemetrySnapshots[0];
            foreach (TelemetrySnapshot snap in LastReceivedTelemetrySnapshots)
            {
                if (snap.CapturedAtUtc > most_recent.CapturedAtUtc)
                {
                    most_recent = snap;
                }
            }

            //Take out any that need to go
            List<TelemetrySnapshot> ToRemove = new List<TelemetrySnapshot>();
            foreach (TelemetrySnapshot snap in LastReceivedTelemetrySnapshots)
            {
                TimeSpan time_since = most_recent.CapturedAtUtc - snap.CapturedAtUtc;
                if (time_since > BufferGuidelines)
                {
                    ToRemove.Add(snap);
                }
            }

            //Take them out
            foreach (TelemetrySnapshot snapshot in ToRemove)
            {
                LastReceivedTelemetrySnapshots.Remove(snapshot);
            }
        }

        public float CurrentSpeedMph
        {
            get
            {
                List<float> MphCalculations = new List<float>();
                if (LastReceivedTelemetrySnapshots != null)
                {
                    //Go through each
                    foreach (TelemetrySnapshot ts in LastReceivedTelemetrySnapshots)
                    {
                        if (ts.Latitude.HasValue && ts.Longitude.HasValue)
                        {
                            foreach (TelemetrySnapshot ToCompareTo in LastReceivedTelemetrySnapshots)
                            {
                                if (ToCompareTo != ts)
                                {
                                    if (ToCompareTo.Latitude.HasValue && ToCompareTo.Longitude.HasValue)
                                    {
                                        TimeSpan TimeDifference = ts.CapturedAtUtc - ToCompareTo.CapturedAtUtc;
                                        if (TimeDifference.Milliseconds > 250)
                                        {
                                            Geolocation loc1 = new Geolocation();
                                            loc1.Latitude = ts.Latitude.Value;
                                            loc1.Longitude = ts.Longitude.Value;
                                            Geolocation loc2 = new Geolocation();
                                            loc2.Latitude = ToCompareTo.Latitude.Value;
                                            loc2.Longitude = ToCompareTo.Longitude.Value;

                                            Distance d = GeoToolkit.MeasureDistance(loc1, loc2);
                                            if (d.Miles > 0)
                                            {
                                                float mph = d.Miles / Convert.ToSingle(TimeDifference.TotalHours);
                                                MphCalculations.Add(mph);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (MphCalculations.Count == 0)
                {
                    return 0f;
                }
                else
                {
                    
                    float stdev = TimHanewich.Toolkit.MathToolkit.StandardDeviation(MphCalculations.ToArray());

                    List<float> ToConsiderForMphAvgCalc = new List<float>();
                    foreach (float val in MphCalculations)
                    {
                        float zScore = Math.Abs(val - MphCalculations.Average()) / stdev;
                        if (zScore < 1.5)
                        {
                            ToConsiderForMphAvgCalc.Add(val);
                        }
                    }

                    if (ToConsiderForMphAvgCalc.Count > 0)
                    {
                        return ToConsiderForMphAvgCalc.Average();
                    }
                    else
                    {
                        return 0;
                    }
                    
                }
            }
        }

        public StationaryStop[] Stops
        {
            get
            {
                return _Stops.ToArray();
            }
        }
    
        public float TopSpeedMph
        {
            get
            {
                return _TopSpeedMph;
            }
        }

        public Guid TopSpeedDetectedAt
        {
            get
            {
                return _TopSpeedDetectedAt;
            }
        }
    }
}