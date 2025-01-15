using Microsoft.AspNetCore.Mvc;
using TrekingTIme.DTO.Employee;
using TrekingTIme.Models;
using TrekingTIme;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeDto dto)
    {
        var companyId = GetCompanyIdFromToken(); 

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CompanyId = companyId,
            HourlyRate = dto.HourlyRate 
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee added successfully." });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetEmployees()
    {
        var companyId = GetCompanyIdFromToken(); 

        var employees = await _context.Employees
            .Where(e => e.CompanyId == companyId)
            .Select(e => new EmployeeDetailsDto
            {
                EmployeeId = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                CompanyName = e.Company.Name,
                HourlyRate = e.HourlyRate 
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetEmployeeDetails(int employeeId)
    {
        var employee = await _context.Employees
            .Where(e => e.EmployeeId == employeeId)
            .Select(e => new EmployeeDetailsDto
            {
                EmployeeId = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                CompanyName = e.Company.Name,
                HourlyRate = e.HourlyRate
            })
            .FirstOrDefaultAsync();

        if (employee == null)
        {
            return NotFound(new { message = "Employee not found." });
        }

        return Ok(employee);
    }
    private int GetCompanyIdFromToken()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        if (identity != null)
        {
            var companyIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(companyIdClaim, out var companyId))
            {
                return companyId;
            }
        }
        throw new UnauthorizedAccessException("Invalid token.");
    }
    [HttpPut("{employeeId}")]
    [Authorize]
    public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] UpdateEmployeeDto dto)
    {
        var companyId = GetCompanyIdFromToken();

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId);

        if (employee == null)
        {
            return NotFound(new { message = "Employee not found or does not belong to your company." });
        }

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.HourlyRate = dto.HourlyRate;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Employee updated successfully." });
    }


}
