namespace FEQuanLyNhanSu.ResponseModels
{
    public class Payrolls
    {
        public class PayrollResultDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public double Salary { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Note { get; set; }
            public int DaysWorked { get; set; }
        }
        public class CreatePayrollDto
        {
            public Guid UserId { get; set; }
            public string Note { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class UpdatePayrollDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public string Note { get; set; }
        }
    }
}
