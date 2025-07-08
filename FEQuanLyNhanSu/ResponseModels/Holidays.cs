using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class Holidays
    {
        public class HolidayResultDto
        {
            public Guid HolidayId { get; set; }
            public string Name { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
        }
        public class HolidayResponse
        {
            public string Message { get; set; }
            public HolidayResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
