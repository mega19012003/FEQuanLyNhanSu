using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;
using static FEQuanLyNhanSu.Services.Checkins;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class Payrolls
    {
        public class PayrollResultDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            //public double Salary { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Note { get; set; }
            public int DaysWorked { get; set; }
        }
        public class PayrollResponse
        {
            public string Message { get; set; }
            public PayrollResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
        public class UserWithPayrollDto
        {
            public Guid UserId { get; set; }
            public string Fullname { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string ImageUrl { get; set; }
            public List<PayrollResultDto> Payrolls { get; set; } = new List<PayrollResultDto>();
        }
    }
}
