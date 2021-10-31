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
    }
}