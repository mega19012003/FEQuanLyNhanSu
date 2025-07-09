using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class Holidays
    {
        public class HolidayResultDto
        {
            public Guid HolidayId { get; set; }
            public string Name { get; set; }
            public DateOnly startDate { get; set; }
            public DateOnly endDate { get; set; }
        }
        public class HolidayResponse
        {
            public string Message { get; set; }
            public HolidayResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
