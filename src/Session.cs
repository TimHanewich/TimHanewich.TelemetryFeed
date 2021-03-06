using System;

namespace TimHanewich.TelemetryFeed
{
    public class Session
    {
        public Guid Id {get; set;}
        public Guid Owner {get; set;}
        public string Title {get; set;}
        public DateTime CreatedAtUtc {get; set;}
        public Guid? RightLeanCalibration {get; set;}
        public Guid? LeftLeanCalibration {get; set;}
        public float? IntendedDestinationLatitude {get; set;}
        public float? IntendedDestinationLongitude {get; set;}
        public short? ClientVersionCode {get; set;} //Verion code (Version number) of the client application at the time of the session being created
    }
}