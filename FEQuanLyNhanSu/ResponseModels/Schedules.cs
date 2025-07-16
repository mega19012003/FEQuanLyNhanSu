using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class Schedules
    {
        public class ScheduleDto
        {
            public Guid id { get; set; }
            public TimeOnly StartTimeMorning { get; set; }
            public TimeOnly EndTimeMorning { get; set; }
            public int LogAllowtime { get; set; }
            public TimeOnly StartTimeAfternoon { get; set; }
            public TimeOnly EndTimeAfternoon { get; set; }
            public string CompanyName { get; set; }
        }
    }
}
