using System.Text.Json.Serialization;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class AllowedIPs
    {
        public class IPDto
        {
            public Guid AllowedIPId { get; set; }
            public string IPAddress { get; set; } = string.Empty;
        }
    }
}
