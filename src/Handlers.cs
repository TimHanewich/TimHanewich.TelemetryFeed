using System;
using TimHanewich.TelemetryFeed.Analysis;

namespace TimHanewich.TelemetryFeed
{
    public delegate void AccelerationStatusHandler(AccelerationStatus status);
    public delegate void VelocityChangeHandler(VelocityChange vc);
    public delegate void TelemetrySnapshotHandler(TelemetrySnapshot ts);
}