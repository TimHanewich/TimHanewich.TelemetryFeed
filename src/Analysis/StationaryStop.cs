using System;

namespace TimHanewich.TelemetryFeed.Analysis
{
    public class StationaryStop
    {
        //ID's of the telemetry snapshots where the beginning and ending of the stop was determined
        public Guid Beginning {get; set;}
        public Guid End {get; set;}

        //Location
        public float Latitude {get; set;}
        public float Longitude {get; set;}

        public TimeSpan Duration(TelemetrySnapshot[] all_packets)
        {
            TelemetrySnapshot b = null;
            TelemetrySnapshot e = null;
            foreach (TelemetrySnapshot ts in all_packets)
            {
                if (ts.Id == Beginning)
                {
                    b = ts;
                }
                else if (ts.Id == End)
                {
                    e = ts;
                }
            }
            if (b == null || e == null)
            {
                throw new Exception("Unable to complete duration measurement. Unable to find both beginning and end snapshot in provided array.");
            }
            TimeSpan ToReturn = e.CapturedAtUtc - b.CapturedAtUtc;
            return ToReturn;
        }
    }
}