namespace FEQuanLyNhanSu.Models.Configs
{
    public class Holiday 
    {
        public Guid HolidayId { get; set; }
        public string name { get; set; }
        public DateOnly startDate { get; set; }
        public DateOnly endDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
