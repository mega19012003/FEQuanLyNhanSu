using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Models.Users;

namespace FEQuanLyNhanSu.Models.Checkins
{
    public class Checkin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public DateTime CheckinDate { get; set; } 
        public DateTime CheckoutDate { get; set; } 
        public LogStatus CheckinStatus { get; set; } = LogStatus.OnTime;
        public LogStatus CheckoutStatus { get; set; } = LogStatus.OnTime;
        public double SalaryPerDay { get; set; } = 0.0;
        public bool IsDeleted { get; set; } = false;
        //public string updateBy { get; set; } 
        //public DateTime UpdateAt { get; set; }
    }
}
