using System;
using System.Collections.Generic;
using TimHanewich.Toolkit;
using TimHanewich.Csv;
using TimHanewich.Toolkit.Geo;

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

        public static string ToCsv(this TelemetrySnapshot[] snapshots, bool IncludeRowNumber = false, bool CapturedAtInSeconds = false, bool IncludeDistance = false)
        {
            CsvFile csv = new CsvFile();
            
            //Add header row
            DataRow header = csv.AddNewRow();
            if (IncludeRowNumber)
            {
                header.Values.Add("RowNumber");
            }
            header.Values.Add("Id");
            header.Values.Add("FromSession");
            header.Values.Add("AccelerationX");
            header.Values.Add("AccelerationY");
            header.Values.Add("AccelerationZ");
            header.Values.Add("GyroscopeX");
            header.Values.Add("GyroscopeY");
            header.Values.Add("GyroscopeZ");
            header.Values.Add("MagnetoX");
            header.Values.Add("MagnetoY");
            header.Values.Add("MagnetoZ");
            header.Values.Add("Latitude");
            header.Values.Add("Longitude");
            header.Values.Add("CapturedAtUtc");
            header.Values.Add("OrientationX");
            header.Values.Add("OrientationY");
            header.Values.Add("OrientationZ");
            header.Values.Add("GpsAccuracy");
            if (IncludeDistance)
            {
                header.Values.Add("DistanceMiles");
            }

            //Get the oldest
            DateTime OldestUtc = DateTime.UtcNow;
            foreach (TelemetrySnapshot ts in snapshots)
            {
                if (ts.CapturedAtUtc < OldestUtc)
                {
                    OldestUtc = ts.CapturedAtUtc;
                }
            }


            //add each row
            int rn = 1;
            foreach (TelemetrySnapshot ts in snapshots)
            {
                DataRow dr = csv.AddNewRow();
                if (IncludeRowNumber)
                {
                    dr.Values.Add(rn.ToString());
                }
                dr.Values.Add(ts.Id.ToString());
                dr.Values.Add(ts.FromSession.ToString());
                
                //Acceleration
                if (ts.AccelerationX.HasValue)
                {
                    dr.Values.Add(ts.AccelerationX.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.AccelerationY.HasValue)
                {
                    dr.Values.Add(ts.AccelerationY.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.AccelerationZ.HasValue)
                {
                    dr.Values.Add(ts.AccelerationZ.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }

                //Gyroscope
                if (ts.GyroscopeX.HasValue)
                {
                    dr.Values.Add(ts.GyroscopeX.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.GyroscopeY.HasValue)
                {
                    dr.Values.Add(ts.GyroscopeY.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.GyroscopeZ.HasValue)
                {
                    dr.Values.Add(ts.GyroscopeZ.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }

                //Magneto
                if (ts.MagnetoX.HasValue)
                {
                    dr.Values.Add(ts.MagnetoX.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.MagnetoY.HasValue)
                {
                    dr.Values.Add(ts.MagnetoY.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.MagnetoZ.HasValue)
                {
                    dr.Values.Add(ts.MagnetoZ.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }

                //Lat + long
                if (ts.Latitude.HasValue)
                {
                    dr.Values.Add(ts.Latitude.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.Longitude.HasValue)
                {
                    dr.Values.Add(ts.Longitude.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }

                //CapturedAtutc
                if (CapturedAtInSeconds)
                {
                    TimeSpan TimeSince = ts.CapturedAtUtc - OldestUtc;
                    dr.Values.Add(TimeSince.TotalSeconds.ToString());
                }
                else
                {
                    dr.Values.Add(ts.CapturedAtUtc.ToString());
                }


                //Orientation
                if (ts.OrientationX.HasValue)
                {
                    dr.Values.Add(ts.OrientationX.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.OrientationY.HasValue)
                {
                    dr.Values.Add(ts.OrientationY.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }
                if (ts.OrientationZ.HasValue)
                {
                    dr.Values.Add(ts.OrientationZ.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }


                //Gps Accuracy
                if (ts.GpsAccuracy.HasValue)
                {
                    dr.Values.Add(ts.GpsAccuracy.Value.ToString());
                }
                else
                {
                    dr.Values.Add("");
                }

                //Include distance?
                if (IncludeDistance)
                {
                    if (rn == 1) //If it is the first row, just show 0
                    {
                        dr.Values.Add("");
                    }
                    else
                    {
                        TelemetrySnapshot last = snapshots[rn - 2];
                        if (last.Latitude.HasValue && last.Longitude.HasValue && ts.Latitude.HasValue && ts.Longitude.HasValue)
                        {
                            Geolocation loc1 = new Geolocation();
                            loc1.Latitude = last.Latitude.Value;
                            loc1.Longitude = last.Longitude.Value;
                            Geolocation loc2 = new Geolocation();
                            loc2.Latitude = ts.Latitude.Value;
                            loc2.Longitude = ts.Longitude.Value;
                            Distance d = GeoToolkit.MeasureDistance(loc1, loc2);
                            dr.Values.Add(d.Miles.ToString());
                        }
                        else
                        {
                            dr.Values.Add("");
                        }
                    }
                }

                //INCREMENTE ROW #
                rn = rn + 1;
            }


            return csv.GenerateAsCsvFileContent();
        }

        
    }
}