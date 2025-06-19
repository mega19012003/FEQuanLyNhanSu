using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FEQuanLyNhanSu.Models.Configs
{
    public class ScheduleTime
    {
        [Key]
        [JsonIgnore]
        public Guid id { get; set; }
        public TimeOnly StartTime { get; set; } //= TimeOnly.MinValue;
        public int LateThresholdMinutes { get; set; } //= 0;
        public TimeOnly EndTime { get; set; } //= TimeOnly.MaxValue;
    }
}
