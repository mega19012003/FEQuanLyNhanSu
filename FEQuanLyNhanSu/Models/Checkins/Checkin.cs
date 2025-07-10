using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Models.Users;

namespace FEQuanLyNhanSu.Models.Checkins
{
    public class Checkin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public DateTime CheckinTime { get; set; }
        public DateTime CheckoutTime { get; set; }
        public Enums.LogStatus LogStatus { get; set; }
        public double SalaryPerDay { get; set; }
        public bool IsDeleted { get; set; } 
    }

}
