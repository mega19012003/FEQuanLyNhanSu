namespace FEQuanLyNhanSu.ResponseModels
{
    public static class Departments
    {
        public class DepartmentResultDto
        {
            public Guid DepartmentId { get; set; }
            public string Name { get; set; }
            public string CompanyName { get; set; }
        }
        public class DepartmentResponse
        {
            public string Message { get; set; }
            public DepartmentResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
        public class PositionByDepartmentDto
        {
            public Guid PositionId { get; set; }
            public string PositionName { get; set; }
            public string DepartmentName { get; set; }
        }

        public class UserFilterDto
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            //public double SalaryPerHour { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}
