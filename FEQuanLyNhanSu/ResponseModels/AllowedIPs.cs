using System.Text.Json.Serialization;
using static FEQuanLyNhanSu.ResponseModels.Departments;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class AllowedIPs
    {
        public class IPResultDto
        {
            public Guid AllowedIPId { get; set; }
            public string IPAddress { get; set; } = string.Empty;
        }
        public class IPResponse
        {
            public string Message { get; set; }
            public IPResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
