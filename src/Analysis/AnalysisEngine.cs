using System;
using TimHanewich.TelemetryFeed;
using TimHanewich.Toolkit;
using TimHanewich.Toolkit.Geo;
using System.Collections.Generic;

namespace TimHanewich.TelemetryFeed.Analysis
{
    public class AnalysisEngine
    {
        //LAST RECEIVED
        private TelemetrySnapshot LastReceivedTelemetrySnapshot;

        //for stops
        private TelemetrySnapshot ZeroDistanceCoveredFirstNoticed;
        private RiderStatus _Status;
        private List<StationaryStop> _Stops = new List<StationaryStop>();

        //Top speed
        private float _TopSpeedMph;
        private Guid _TopSpeedDetectedAt; //Guid of telemetry snapshot where the top speed was detected


        public void Feed(TelemetrySnapshot ts)
        {
            if (LastReceivedTelemetrySnapshot != null)
            {
                if (ts.Latitude.HasValue && ts.Longitude.HasValue && LastReceivedTelemetrySnapshot.Latitude.HasValue && LastReceivedTelemetrySnapshot.Longitude.HasValue)
                {
                    //Measure distance
                    Geolocation loc1 = new Geolocation();
                    loc1.Latitude = LastReceivedTelemetrySnapshot.Latitude.Value;
                    loc1.Longitude = LastReceivedTelemetrySnapshot.Longitude.Value;
                    Geolocation loc2 = new Geolocation();
                    loc2.Latitude = ts.Latitude.Value;
                    loc2.Longitude = ts.Longitude.Value;
                    Distance d = GeoToolkit.MeasureDistance(loc1, loc2);
                    
                    //Calculate stops
                    if (d.Miles == 0)
                    {
                        if (_Status == RiderStatus.Moving)
                        {
                            //Only officially make this stationary (flip to stationary) if the stop was more than a certain length of time
                            if (ZeroDistanceCoveredFirstNoticed == null)
                            {
                                ZeroDistanceCoveredFirstNoticed = ts;
                            }
                            else
                            {
                                TimeSpan TimeSinceFirstNoMove = ts.CapturedAtUtc - ZeroDistanceCoveredFirstNoticed.CapturedAtUtc;
                                if (TimeSinceFirstNoMove.TotalSeconds > 4)
                                {
                                    _Status = RiderStatus.Stationary;
                                }
                            }
                        }
                        else if (_Status == RiderStatus.Stationary)
                        {
                            //Do nothing (wait until we start moving again)
                        }
                    }
                    else if (d.Miles > 0)
                    {
                        if (_Status == RiderStatus.Stationary) //If this was marked as stationary, need to update it
                        {
                            _Status = RiderStatus.Moving; //Change to moving
                            if (ZeroDistanceCoveredFirstNoticed != null)
                            {
                                StationaryStop ss = new StationaryStop();
                                ss.Beginning = ZeroDistanceCoveredFirstNoticed.Id;
                                ss.End = ts.Id;
                                ss.Latitude = ZeroDistanceCoveredFirstNoticed.Latitude.Value;
                                ss.Longitude = ZeroDistanceCoveredFirstNoticed.Longitude.Value;
                                _Stops.Add(ss);

                                ZeroDistanceCoveredFirstNoticed = null; //Set the zero distance covered to null.
                            }
                        }
                        ZeroDistanceCoveredFirstNoticed = null;
                    }

                    //Calculate this speed
                    if (d.Miles > 0)
                    {
                        TimeSpan timelapsed = ts.CapturedAtUtc - LastReceivedTelemetrySnapshot.CapturedAtUtc;
                        float mph = d.Miles / Convert.ToSingle(timelapsed.TotalHours);
                        if (mph > _TopSpeedMph)
                        {
                            _TopSpeedMph = mph;
                            _TopSpeedDetectedAt = ts.Id;
                        }
                    }

                }
            }
            
            LastReceivedTelemetrySnapshot = ts;
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