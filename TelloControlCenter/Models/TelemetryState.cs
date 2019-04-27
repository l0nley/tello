namespace TelloControlCenter.Models
{
    public class TelemetryState
    {
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float Yaw { get; set; }
        public float SpeedX { get; set; }
        public float SpeedY { get; set; }
        public float SpeedZ { get; set; }
        public float AccelX { get; set; }
        public float AccelY { get; set; }
        public float AccelZ { get; set; }
        public int Battery { get; set; }
        public float Temperature { get; set; }
        public float Height { get; set; }
        public float Barometer { get; set; }
        public float TOF { get; set; }
        public int FlyTime { get; set; }
       
    }
}
