using System;
using TimHanewich.TelemetryFeed;
using TimHanewich.Toolkit;
using TimHanewich.Toolkit.Geo;
using System.Collections.Generic;
using System.Linq;

namespace TimHanewich.TelemetryFeed.Analysis
{
    public class AnalysisEngine
    {
        //LAST RECEIVED 
        private TelemetrySnapshot LastReceivedTelemetrySnapshot;

        //Earliest received and latest received (for total time)
        private TelemetrySnapshot OldestReceived;
        private TelemetrySnapshot YoungestReceived;

        //for stops
        private TelemetrySnapshot StationaryFirstNoticed;
        private RiderStatus _Status;
        private List<StationaryStop> _Stops = new List<StationaryStop>();
        public event RiderStatusHandler RiderStatusChanged;

        //Top speed
        public event TelemetrySnapshotHandler NewTopSpeedReached;
        private float? _TopSpeedMph;
        private Guid? _TopSpeedDetectedAt; //Guid of telemetry snapshot where the top speed was detected

        

        public void Feed(TelemetrySnapshot ts)
        {
            //First seen? Last seen?
            if (OldestReceived != null)
            {
                if (ts.CapturedAtUtc < OldestReceived.CapturedAtUtc)
                {
                    OldestReceived = ts;
                }
            }
            else
            {
                OldestReceived = ts;
            }
            if (YoungestReceived != null)
            {
                if (ts.CapturedAtUtc > YoungestReceived.CapturedAtUtc)
                {
                    YoungestReceived = ts;
                }
            }
            else
            {
                YoungestReceived = ts;
            }

            //Check for top speed
            if (CurrentSpeedMph.HasValue)
            {
                float speed = CurrentSpeedMph.Value;
                if (_TopSpeedMph.HasValue)
                {
                    if (speed > _TopSpeedMph.Value)
                    {
                        _TopSpeedMph = speed;
                        _TopSpeedDetectedAt = ts.Id;

                        //Raise the event
                        try
                        {
                            NewTopSpeedReached.Invoke(ts);
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    _TopSpeedMph = speed;
                    _TopSpeedDetectedAt = ts.Id;
                }
            }
            

            //Check for stop
            if (CurrentSpeedMph.HasValue)
            {
                if (CurrentSpeedMph.Value < 2) //SPEEDS BELOW THIS INDICATE a 'stop'
                {
                    if (_Status == RiderStatus.Moving)
                    {
                        _Status = RiderStatus.Stationary; //Mark as stationary
                        StationaryFirstNoticed = ts; //Mark the first time we are seeing it is stationary. 
                        TryRaiseRiderStatusChanged(_Status); //try to raise the event (if subscribed to)
                    }
                }
                else //We are over the minimum speed and are now moving
                {
                    if (_Status == RiderStatus.Stationary) //If it was previously marked as stationary
                    {
                        TimeSpan TimeSinceStationaryBegan = ts.CapturedAtUtc - StationaryFirstNoticed.CapturedAtUtc;
                        StationaryStop ss = new StationaryStop();
                        ss.Beginning = StationaryFirstNoticed.Id;
                        ss.End = ts.Id;
                        if (StationaryFirstNoticed.Latitude.HasValue)
                        {
                            ss.Latitude = StationaryFirstNoticed.Latitude.Value;
                        }
                        if (StationaryFirstNoticed.Longitude.HasValue)
                        {
                            ss.Longitude = StationaryFirstNoticed.Longitude.Value;
                        }
                        ss.BeganAtUtc = StationaryFirstNoticed.CapturedAtUtc;
                        ss.EndedAtUtc = ts.CapturedAtUtc;
                        if (ss.Duration.TotalSeconds >= 2)
                        {
                            _Stops.Add(ss);
                        }
                        
                        
                        //Flip and clear
                        _Status = RiderStatus.Moving;
                        StationaryFirstNoticed = null;
                        TryRaiseRiderStatusChanged(_Status); //try to raise the event (if subscribed to)
                    }
                }
            }


            #region "Acceleration"

            int OptimalBuffer = 6; //The number of telemetry snapshots to use to calculate (average out the acceleration)
            TimeSpan MinimumTimeSpanToBelieveAcceleration = TimeSpan.FromMilliseconds(200);
            bool TrimAnomolies = false; //Should the data be put through a z-score/st dev calculation to trim the anomolies?
            float TrimAnomolyZScoresBeyond = 1.2f; //Values with a z score beyond this (positive or negative) will be trimmed.


            //If it is null, make one
            if (BufferForAcceleration == null)
            {
                BufferForAcceleration = new List<TelemetrySnapshot>();
            }


            //Add it
            BufferForAcceleration.Add(ts);

            //Arrange
            BufferForAcceleration = TelemetrySnapshot.OldestToNewest(BufferForAcceleration.ToArray()).ToList();

            //Remove the oldest until we get to only 10
            while (BufferForAcceleration.Count > OptimalBuffer)
            {
                BufferForAcceleration.RemoveAt(0);
            }


            //Get the average change
            List<float> Changes = new List<float>();
            if (BufferForAcceleration.Count >= OptimalBuffer)
            {
                for (int i = 0; i < BufferForAcceleration.Count - 1; i++)
                {
                    TelemetrySnapshot snap1 = BufferForAcceleration[i];
                    TelemetrySnapshot snap2 = BufferForAcceleration[i + 1];
                    if (snap1.SpeedMetersPerSecond.HasValue && snap2.SpeedMetersPerSecond.HasValue)
                    {
                        float SpeedDiff = snap2.SpeedMetersPerSecond.Value - snap1.SpeedMetersPerSecond.Value;
                        TimeSpan span = snap2.CapturedAtUtc - snap1.CapturedAtUtc;
                        if (span >= MinimumTimeSpanToBelieveAcceleration)
                        {
                            float SpeedDiffTimes = SpeedDiff / Convert.ToSingle(span.TotalSeconds);
                            Changes.Add(SpeedDiffTimes);
                        }
                    }
                }
            }

            //Throw out the anomolies? (only need to trim if there is data)
            if (Changes.Count > 2)
            {
                if (TrimAnomolies)
                {
                    float StDev = TimHanewich.Toolkit.MathToolkit.StandardDeviation(Changes.ToArray());
                    List<float> ToUse = new List<float>();
                    foreach (float f in Changes)
                    {
                        float zscore = (f - Changes.Average()) / StDev;
                        if (Math.Abs(zscore) <= TrimAnomolyZScoresBeyond)
                        {
                            ToUse.Add(f);
                        }
                    }
                    Changes = ToUse.ToList();
                }
            }
            


            //Return
            if (Changes.Count > 0)
            {
                AddToAccelerationMPS2Buffer(Changes.Average());
            }  


            #endregion

            #region "Acceleration Status"

            //ACCELERATION/DECELERATION SETTINGS
            float MAccelerating = 0.5f; //Change in velocity (in meters per second) equal to or greater than this is considered "accelerating"
            float MDecelerating = -0.5f; //Change in velocity (in meters per second) equal to or less than this is considered "decelerating"
            TimeSpan MinimumVelocityChangeDuration = TimeSpan.FromSeconds(1); //If a change in velocity lasted less than this duration, it will not be registered as a velocity change
            float MinimumMetersPerSecondChange = 5f; //If a change is velocity in total is less than this swing (up or down) in speed, it will not be registered as a velocity change


            //Make the list if it isn't there
            if (_VelocityChanges == null)
            {
                _VelocityChanges = new List<VelocityChange>();
            }

            if (AccelerationMPS2.HasValue)
            {
                if (_AccelerationStatus == AccelerationStatus.MaintainingSpeed)
                {
                    if (AccelerationMPS2.Value >= MAccelerating || AccelerationMPS2.Value <= MDecelerating)
                    {
                        //Start a new velocity change
                        CurrentVelocityChange = new VelocityChange();
                        CurrentVelocityChange.BeginningSnapshot = ts.Id;
                        CurrentVelocityChange.BeginningSpeedMetersPerSecond = ts.SpeedMetersPerSecond.Value;
                        CurrentVelocityChange.BeginningUtc = ts.CapturedAtUtc;

                        //Mark the status
                        if (AccelerationMPS2.Value >= MAccelerating)
                        {
                            _AccelerationStatus = AccelerationStatus.Accelerating;
                            TryRaiseAccelerationStatusChanged(_AccelerationStatus);
                        }
                        else if (AccelerationMPS2.Value <= MDecelerating)
                        {
                            _AccelerationStatus = AccelerationStatus.Decelerating;
                            TryRaiseAccelerationStatusChanged(_AccelerationStatus);
                        }
                    }
                }
                else //Current status is accelerating or decelerating
                {

                    //Is this acceleration or decelerartion over?
                    bool VelocityChangeIsOver = false;
                    if (_AccelerationStatus == AccelerationStatus.Accelerating)
                    {
                        if (AccelerationMPS2.Value < MAccelerating)
                        {
                            VelocityChangeIsOver = true;
                        }
                    }
                    else if (_AccelerationStatus == AccelerationStatus.Decelerating)
                    {
                        if (AccelerationMPS2.Value > MDecelerating)
                        {
                            VelocityChangeIsOver = true;
                        }
                    }


                    //If this velocity change is over, end it.
                    if (VelocityChangeIsOver)
                    {
                        
                        //Only actually book the change if the duration of this acceleration or deceleration is over a certain amount
                        TimeSpan DurationOfThisPossibleVelocityChange = ts.CapturedAtUtc - CurrentVelocityChange.BeginningUtc;
                        if (DurationOfThisPossibleVelocityChange >= MinimumVelocityChangeDuration)
                        {
                            if (ts.SpeedMetersPerSecond.HasValue)
                            {
                                //Also only book the change if the ending speed is not the same as the beginning speed (this would NOT be a velocity change)
                                if (ts.SpeedMetersPerSecond.Value != CurrentVelocityChange.BeginningSpeedMetersPerSecond)
                                {

                                    //Also only book the change if the difference in speeds is more than a certain amount
                                    if (Math.Abs(ts.SpeedMetersPerSecond.Value - CurrentVelocityChange.BeginningSpeedMetersPerSecond) >= MinimumMetersPerSecondChange)
                                    {
                                        //Book the change
                                        CurrentVelocityChange.EndingSnapshot = ts.Id;
                                        CurrentVelocityChange.EndingSpeedMetersPerSecond = ts.SpeedMetersPerSecond.Value;
                                        CurrentVelocityChange.EndingUtc = ts.CapturedAtUtc;
                                        
                                        //Raise the event (well, try to)
                                        TryRaiseVelocityChangeRecorded(CurrentVelocityChange);

                                        //Add it to the list and move on (prepare for the next one)
                                        _VelocityChanges.Add(CurrentVelocityChange);
                                        CurrentVelocityChange = null;
                                        

                                        
                                    }
                                }  
                            }
                        }  


                        //Mark the status
                        _AccelerationStatus = AccelerationStatus.MaintainingSpeed;
                        TryRaiseAccelerationStatusChanged(_AccelerationStatus);

                    } 
                }
            }

            #endregion


            //SET LAST RECEIVED!
            LastReceivedTelemetrySnapshot = ts;
            
        }

        public RiderStatus Status
        {
            get
            {
                return _Status;
            }
        }

        public float? CurrentSpeedMph
        {
            get
            {
                if (LastReceivedTelemetrySnapshot != null)
                {
                    if (LastReceivedTelemetrySnapshot.SpeedMetersPerSecond.HasValue)
                    {
                        return LastReceivedTelemetrySnapshot.SpeedMetersPerSecond.Value * 2.23694f;
                    }
                }
                return null; //return null if the above didn't work.
            }
        }

        public float? AccelerationMPS2
        {
            get
            {
                if (AccelerationMPS2Buffer.Count > 1)
                {
                    return AccelerationMPS2Buffer.Average();
                }
                else
                {
                    return null;
                }
                
            }
        }

        public StationaryStop[] Stops
        {
            get
            {
                return _Stops.ToArray();
            }
        }
    
        public float? TopSpeedMph
        {
            get
            {
                return _TopSpeedMph;
            }
        }

        public Guid? TopSpeedDetectedAt
        {
            get
            {
                return _TopSpeedDetectedAt;
            }
        }
    

        #region "Acceleration/velocity changing"


        //Buffer for calculating the acceleration (rolling average)
        private int OptimalAccelerationBufferLength = 2;
        private List<float> AccelerationMPS2Buffer = new List<float>(); //Arranged from oldest to newest


        //Acceleration change
        public event AccelerationStatusHandler AccelerationStatusChanged;
        public event VelocityChangeHandler VelocityChangeRecorded;
        private List<TelemetrySnapshot> BufferForAcceleration;
        private AccelerationStatus _AccelerationStatus;
        private List<VelocityChange> _VelocityChanges;
        private VelocityChange CurrentVelocityChange;


        public AccelerationStatus AccelerationStatus
        {
            get
            {
                return _AccelerationStatus;
            }
        }

        public VelocityChange[] VelocityChanges
        {
            get
            {
                if (_VelocityChanges == null)
                {
                    return new VelocityChange[]{};
                }
                else
                {
                    return _VelocityChanges.ToArray();
                }
            }
        }

        private void TryRaiseAccelerationStatusChanged(AccelerationStatus status)
        {
            try
            {
                AccelerationStatusChanged.Invoke(status);
            }
            catch
            {

            }
        }

        private void TryRaiseVelocityChangeRecorded(VelocityChange vc)
        {
            try
            {
                VelocityChangeRecorded.Invoke(vc);
            }
            catch
            {

            }
        }

        private void AddToAccelerationMPS2Buffer(float value)
        {
            AccelerationMPS2Buffer.Add(value);
            
            //Trim
            while (AccelerationMPS2Buffer.Count > OptimalAccelerationBufferLength)
            {
                AccelerationMPS2Buffer.RemoveAt(0);
            }
        }

        #endregion

        #region "Riding timing statistics"

        public TimeSpan TotalTime
        {
            get
            {
                if (OldestReceived != null && YoungestReceived != null)
                {
                    TimeSpan ts = YoungestReceived.CapturedAtUtc - OldestReceived.CapturedAtUtc;
                    return ts;
                }
                else
                {
                    return new TimeSpan(0, 0, 0);
                }
            }
        }
    
        public TimeSpan TotalTimeStationary
        {
            get
            {
                TimeSpan ToAddTo = new TimeSpan(0, 0, 0);
                foreach (StationaryStop ss in Stops)
                {
                    TimeSpan StopDuration = ss.Duration;
                    ToAddTo = ToAddTo + StopDuration;
                }
                return ToAddTo;
            }
        }

        public TimeSpan TotalTimeMoving
        {
            get
            {
                TimeSpan tt = TotalTime;
                TimeSpan ts = TotalTimeStationary;
                return tt - ts;
            }
        }
    
        public float PercentTimeStationary
        {
            get
            {
                TimeSpan total = TotalTime;
                TimeSpan stationary = TotalTimeStationary;
                float ToReturn = Convert.ToSingle(stationary.TotalSeconds) / Convert.ToSingle(total.TotalSeconds);
                return ToReturn;
            }
        }

        public float PercentTimeMoving
        {
            get
            {
                float PercentStationary = PercentTimeStationary;
                return 1f - PercentStationary;
            }
        }

        #endregion
    
        #region "Toolkit"

        private void TryRaiseRiderStatusChanged(RiderStatus rs)
        {
            try
            {
                RiderStatusChanged.Invoke(rs);
            }
            catch
            {
                
            }
        }

        #endregion

    }
}