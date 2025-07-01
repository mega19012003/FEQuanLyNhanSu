namespace FEQuanLyNhanSu.Models.Configs
{
    public class Holiday 
    {
        public Guid HolidayId { get; set; }
        public string name { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
