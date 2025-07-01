using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FEQuanLyNhanSu.Models.Configs
{
    public class ScheduleTime
    {
        public Guid id { get; set; }
        public TimeOnly StartTimeMorning { get; set; }
        public TimeOnly EndTimeMorning { get; set; }
        public int LogAllowtime { get; set; }
        public int LateThresholdMinutes { get; set; }
        public TimeOnly StartTimeAfternoon { get; set; }
        public TimeOnly EndTimeAfternoon { get; set; }
    }
}
