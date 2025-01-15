namespace TrekingTIme.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public decimal HourlyRate { get; set; }
        public ICollection<WorkHour> WorkHours { get; set; }
    }
}
