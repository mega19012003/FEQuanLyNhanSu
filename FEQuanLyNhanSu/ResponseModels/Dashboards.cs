using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class Dashboards
    {
        public class DashboardOverviewDto
        {
            public int TotalEmployees { get; set; }
            public int ActiveEmployees { get; set; }
            public int TotalDepartments { get; set; }
            public int TotalPositions { get; set; }
            public int CheckinLateCountToday { get; set; }
            public int TotalCheckinsToday { get; set; }
            public decimal TotalPayrollThisMonth { get; set; }
            public List<UpcomingHolidayDto> UpcomingHolidays { get; set; }
        }

        public class UpcomingHolidayDto
        {
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }

        //public class ApiResponse<T>
        //{
        //    [JsonProperty("message")]
        //    public string Message { get; set; }
        //    [JsonProperty("data")]
        //    public T Data { get; set; }
        //    [JsonProperty("statusCode")]
        //    public int StatusCode { get; set; }
        //}
    }
}
