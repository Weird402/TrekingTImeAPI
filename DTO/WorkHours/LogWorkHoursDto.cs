namespace TrekingTIme.DTO.WorkHours
{
    public class LogWorkHoursDto
    {
        public int EmployeeId { get; set; } 
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
