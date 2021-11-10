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
                    
                    if (d.Miles == 0)
                    {
                        if (_Status == RiderStatus.Moving)
                        {
                            _Status = RiderStatus.Stationary;
                            ZeroDistanceCoveredFirstNoticed = ts;
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
    }
}