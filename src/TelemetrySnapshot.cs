using System;

namespace TimHanewich.TelemetryFeed
{
    public class TelemetrySnapshot
    {
        public Guid Id {get; set;}
        public Guid FromSession {get; set;}
        public DateTime CapturedAtUtc {get; set;}

        //Accelerometer
        public float? AccelerationX {get; set;}
        public float? AccelerationY {get; set;}
        public float? AccelerationZ {get; set;}

        //Gyroscope
        public float? GyroscopeX {get; set;}
        public float? GyroscopeY {get; set;}
        public float? GyroscopeZ {get; set;}

        //Magneto
        public float? MagnetoX {get; set;}
        public float? MagnetoY {get; set;}
        public float? MagnetoZ {get; set;}

        //GPS
        public float? Latitude {get; set;}
        public float? Longitude {get; set;}

    }
}