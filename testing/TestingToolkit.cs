using System;
using TimHanewich.TelemetryFeed;

namespace testing
{
    public class TestingToolkit
    {
        public static TelemetrySnapshot RandomTelemetrySnapshot()
        {
            Random r = new Random();

            TelemetrySnapshot ts = new TelemetrySnapshot();
            ts.Id = Guid.NewGuid();
            ts.FromSession = Guid.Empty;
            ts.CapturedAtUtc = DateTime.UtcNow;
            ts.AccelerationX = Convert.ToSingle(r.NextDouble());
            ts.AccelerationY = Convert.ToSingle(r.NextDouble());
            ts.AccelerationZ = Convert.ToSingle(r.NextDouble());
            ts.GyroscopeX = Convert.ToSingle(r.NextDouble());
            ts.GyroscopeY = Convert.ToSingle(r.NextDouble());
            ts.GyroscopeZ = Convert.ToSingle(r.NextDouble());
            ts.MagnetoX = Convert.ToSingle(r.NextDouble());
            ts.MagnetoY = Convert.ToSingle(r.NextDouble());
            ts.MagnetoZ = Convert.ToSingle(r.NextDouble());
            ts.Latitude = Convert.ToSingle(r.NextDouble());
            ts.Longitude = Convert.ToSingle(r.NextDouble());
            return ts;
        }
    }
}