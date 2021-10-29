using System;
using System.Collections.Generic;
using TimHanewich.Toolkit;

namespace TimHanewich.TelemetryFeed
{
    public class TelemetrySnapshot
    {
        public Guid Id {get; set;} //16 bytes
        public Guid FromSession {get; set;} //16 bytes
        public DateTime CapturedAtUtc {get; set;} //8 bytes

        //Accelerometer
        public float? AccelerationX {get; set;} //4 bytes
        public float? AccelerationY {get; set;} //4 bytes
        public float? AccelerationZ {get; set;} //4 bytes

        //Gyroscope
        public float? GyroscopeX {get; set;} //4 bytes
        public float? GyroscopeY {get; set;} //4 bytes
        public float? GyroscopeZ {get; set;} //4 bytes

        //Magneto
        public float? MagnetoX {get; set;} //4 bytes
        public float? MagnetoY {get; set;} //4 bytes
        public float? MagnetoZ {get; set;} //4 bytes

        //Orientation
        public float? OrientationX {get; set;} //4 bytes
        public float? OrientationY {get; set;} //4 bytes
        public float? OrientationZ {get; set;} //4 bytes

        //GPS
        public float? Latitude {get; set;} //4 bytes
        public float? Longitude {get; set;} //4 bytes

        public byte[] ToBytes()
        {
            List<byte> ToReturn = new List<byte>();
            ToReturn.AddRange(Id.ToByteArray());
            ToReturn.AddRange(FromSession.ToByteArray());
            ToReturn.AddRange(BitConverter.GetBytes(CapturedAtUtc.ToOADate()));
            ToReturn.AddRange(NullableFloatToBytes(AccelerationX));
            ToReturn.AddRange(NullableFloatToBytes(AccelerationY));
            ToReturn.AddRange(NullableFloatToBytes(AccelerationZ));
            ToReturn.AddRange(NullableFloatToBytes(GyroscopeX));
            ToReturn.AddRange(NullableFloatToBytes(GyroscopeY));
            ToReturn.AddRange(NullableFloatToBytes(GyroscopeZ));
            ToReturn.AddRange(NullableFloatToBytes(MagnetoX));
            ToReturn.AddRange(NullableFloatToBytes(MagnetoY));
            ToReturn.AddRange(NullableFloatToBytes(MagnetoZ));
            ToReturn.AddRange(NullableFloatToBytes(OrientationX));
            ToReturn.AddRange(NullableFloatToBytes(OrientationY));
            ToReturn.AddRange(NullableFloatToBytes(OrientationZ));
            ToReturn.AddRange(NullableFloatToBytes(Latitude));
            ToReturn.AddRange(NullableFloatToBytes(Longitude));
            return ToReturn.ToArray();
        }

        public static TelemetrySnapshot FromBytes(byte[] bytes)
        {
            ByteArrayManager BAM = new ByteArrayManager(bytes);
            TelemetrySnapshot ToReturn = new TelemetrySnapshot();
            
            ToReturn.Id = new Guid(BAM.NextBytes(16));
            ToReturn.FromSession = new Guid(BAM.NextBytes(16));
            ToReturn.CapturedAtUtc = DateTime.FromOADate(BitConverter.ToDouble(BAM.NextBytes(8), 0));
            ToReturn.AccelerationX = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.AccelerationY = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.AccelerationZ = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.GyroscopeX = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.GyroscopeY = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.GyroscopeZ = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.MagnetoX = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.MagnetoY = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.MagnetoZ = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.OrientationX = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.OrientationY = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.OrientationZ = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.Latitude = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));
            ToReturn.Longitude = ToReturn.BytesToNullableFloat(BAM.NextBytes(4));

            return ToReturn;
        }

        private byte[] NullableFloatToBytes(float? value)
        {
            if (value.HasValue == false)
            {
                return BitConverter.GetBytes(float.NaN);
            }
            else
            {
                return BitConverter.GetBytes(value.Value);
            }
        }

        private float? BytesToNullableFloat(byte[] bytes)
        {
            float value = BitConverter.ToSingle(bytes, 0);
            if (value == float.NaN)
            {
                return null;
            }
            else
            {
                return value;
            }
        }
    }
}