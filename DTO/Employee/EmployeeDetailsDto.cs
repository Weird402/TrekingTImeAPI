namespace TrekingTIme.DTO.Employee
{
    public class EmployeeDetailsDto
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public decimal HourlyRate { get; internal set; }
    }
}
