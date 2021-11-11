using System;
using System.Collections.Generic;

namespace TimHanewich.TelemetryFeed
{
    public static class TelemetryFeedToolkit
    {
        public static byte[] ToBytes(this TelemetrySnapshot[] snapshots)
        {
            List<byte> ToReturn = new List<byte>();

            foreach (TelemetrySnapshot ts in snapshots)
            {
                ToReturn.AddRange(ts.ToBytes());
            }

            return ToReturn.ToArray();
        }
    }
}