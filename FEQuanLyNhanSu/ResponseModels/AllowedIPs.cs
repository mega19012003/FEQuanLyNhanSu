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

        public class AddIPDto
        {
            [JsonIgnore]
            public Guid AllowedIPId { get; set; }
            public string IPAddress { get; set; } = string.Empty;
        }

        public class updateIPDto
        {
            public Guid AllowedIPId { get; set; }
            public string IPAddress { get; set; } = string.Empty;
        }
    }
}
