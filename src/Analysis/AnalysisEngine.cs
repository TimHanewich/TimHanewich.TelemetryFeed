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
        private TimeSpan BufferGuidelines;
        private List<TelemetrySnapshot> LastReceivedTelemetrySnapshots;

        //for stops
        private TelemetrySnapshot ZeroDistanceCoveredFirstNoticed;
        private RiderStatus _Status;
        private List<StationaryStop> _Stops = new List<StationaryStop>();

        //Top speed
        private float _TopSpeedMph;
        private Guid _TopSpeedDetectedAt; //Guid of telemetry snapshot where the top speed was detected

        public AnalysisEngine()
        {
            LastReceivedTelemetrySnapshots = new List<TelemetrySnapshot>();
            BufferGuidelines = new TimeSpan(0, 0, 7);
        }

        public void Feed(TelemetrySnapshot ts)
        {
            //Add it to the buffer
            AddSnapshotToBuffer(ts);
        }

        private void AddSnapshotToBuffer(TelemetrySnapshot ts)
        {
            if (LastReceivedTelemetrySnapshots == null)
            {
                LastReceivedTelemetrySnapshots = new List<TelemetrySnapshot>();
            }
            LastReceivedTelemetrySnapshots.Add(ts);

            //Find the most recent one
            TelemetrySnapshot most_recent = LastReceivedTelemetrySnapshots[0];
            foreach (TelemetrySnapshot snap in LastReceivedTelemetrySnapshots)
            {
                if (snap.CapturedAtUtc > most_recent.CapturedAtUtc)
                {
                    most_recent = snap;
                }
            }

            //Take out any that need to go
            List<TelemetrySnapshot> ToRemove = new List<TelemetrySnapshot>();
            foreach (TelemetrySnapshot snap in LastReceivedTelemetrySnapshots)
            {
                TimeSpan time_since = most_recent.CapturedAtUtc - snap.CapturedAtUtc;
                if (time_since > BufferGuidelines)
                {
                    ToRemove.Add(snap);
                }
            }

            //Take them out
            foreach (TelemetrySnapshot snapshot in ToRemove)
            {
                LastReceivedTelemetrySnapshots.Remove(snapshot);
            }
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