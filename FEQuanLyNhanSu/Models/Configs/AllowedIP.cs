using EmployeeAPI.Models;

namespace FEQuanLyNhanSu.Models.Configs
{
    public class AllowedIP
    {
        public Guid AllowedIPId { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
