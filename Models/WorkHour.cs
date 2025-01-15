namespace TrekingTIme.Models
{
    public class WorkHour
    {
        public int WorkHourId { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BreakTime { get; set; }
        public bool? Urlab { get; set; } 
        public bool? Krank { get; set; }
        public bool? Feiertag { get; set; } 
        public string Baustelle { get; set; } 
    }
}
