using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class Positions
    {
        public class PositionResultDto 
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string DepartmentName { get; set; }
        }
        public class PositionResponse
        {
            public string Message { get; set; }
            public PositionResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
