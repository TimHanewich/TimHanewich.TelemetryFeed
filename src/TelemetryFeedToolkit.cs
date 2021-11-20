using System;
using System.Collections.Generic;
using TimHanewich.Toolkit;
using TimHanewich.Csv;
using TimHanewich.Toolkit.Geo;
using TimHanewich.TelemetryFeed.Analysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public static string ToCsvContent(this TelemetrySnapshot[] snapshots, bool with_additions)
        {
            
            List<JObject> DataToConvert = new List<JObject>();
            AnalysisEngine ae = new AnalysisEngine();
            foreach (TelemetrySnapshot ts in snapshots)
            {
                JObject ToAdd = JObject.Parse(JsonConvert.SerializeObject(ts));

                //If with additions, feed and write values if they are there.
                if (with_additions)
                {
                    //Feed
                    ae.Feed(ts);

                    //Acceleration
                    if (ae.AccelerationMPS2.HasValue)
                    {
                        ToAdd.Add("AccelerationMPS2", ae.AccelerationMPS2.Value.ToString());
                    }

                    //Status
                    ToAdd.Add("Status", ae.Status.ToString());

                    //Acceleraion status
                    ToAdd.Add("AccelerationStatus", ae.AccelerationStatus.ToString());
                }

                DataToConvert.Add(ToAdd);
            }

            CsvFile ToReturn = CsvToolkit.JsonToCsv(DataToConvert.ToArray());
            return ToReturn.GenerateAsCsvFileContent();            
        }

        
    }
}