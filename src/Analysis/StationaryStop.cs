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
    }
}