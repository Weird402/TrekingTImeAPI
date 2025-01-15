using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrekingTIme.DTO.WorkHours;
using TrekingTIme.Models;
using TrekingTIme;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class WorkHoursController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkHoursController(AppDbContext context)
    {
        _context = context;
    }

    // Додати робочі години співробітнику
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> LogWorkHours([FromBody] LogWorkHoursDto dto)
    {
        var companyId = GetCompanyIdFromToken();

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.CompanyId == companyId);

        if (employee == null)
        {
            return BadRequest(new { message = "Employee not found or does not belong to your company." });
        }

        var workHour = new WorkHour
        {
            EmployeeId = dto.EmployeeId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            BreakTime = dto.BreakTime,
            Urlab = dto.Urlab,
            Krank = dto.Krank,
            Feiertag = dto.Feiertag,
            Baustelle = dto.Baustelle
        };

        _context.WorkHours.Add(workHour);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Work hours logged successfully." });
    }


    // Метод для отримання ID компанії з токена
    private int GetCompanyIdFromToken()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        var companyIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(companyIdClaim, out var companyId))
        {
            return companyId;
        }

        throw new UnauthorizedAccessException("Invalid token.");
    }
    [HttpGet("history/employee/{employeeId}")]
    [Authorize]
    public async Task<IActionResult> GetWorkHoursHistoryByEmployee(int employeeId)
    {
        var companyId = GetCompanyIdFromToken();

        // Перевіряємо, чи співробітник належить до компанії
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId);

        if (employee == null)
        {
            return BadRequest(new { message = "Employee not found or does not belong to your company." });
        }

        // Отримуємо історію годин для співробітника
        var history = _context.WorkHours
            .Where(w => w.EmployeeId == employeeId)
            .AsEnumerable()
            .Select(w => new
            {
                w.Date,
                w.StartTime,
                w.EndTime,
                w.BreakTime,
                w.Urlab,
                w.Krank,
                w.Feiertag,
                w.Baustelle,
                TotalHours = CalculateTotalHours(w.StartTime, w.EndTime, w.BreakTime)
            })
            .OrderBy(w => w.Date)
            .ToList();

        return Ok(history);
    }

    [HttpGet("history/employee/{employeeId}/month/{month}/{year}")]
    [Authorize]
    public async Task<IActionResult> GetWorkHoursByMonthForEmployee(int employeeId, int month, int year)
    {
        var companyId = GetCompanyIdFromToken();

        // Перевіряємо, чи співробітник належить до компанії
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId);

        if (employee == null)
        {
            return BadRequest(new { message = "Employee not found or does not belong to your company." });
        }

        // Отримуємо історію годин для співробітника за місяць
        var history = _context.WorkHours
            .Where(w => w.EmployeeId == employeeId && w.Date.Month == month && w.Date.Year == year)
            .AsEnumerable()
            .Select(w => new
            {
                w.Date,
                w.StartTime,
                w.EndTime,
                w.BreakTime,
                w.Urlab,
                w.Krank,
                w.Feiertag,
                w.Baustelle,
                TotalHours = CalculateTotalHours(w.StartTime, w.EndTime, w.BreakTime)
            })
            .OrderBy(w => w.Date)
            .ToList();

        return Ok(history);
    }


    [HttpGet("history/employee/{employeeId}/day/{date}")]
    [Authorize]
    public async Task<IActionResult> GetWorkHoursByDayForEmployee(int employeeId, DateTime date)
    {
        var companyId = GetCompanyIdFromToken();

        // Перевіряємо, чи співробітник належить до компанії
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId);

        if (employee == null)
        {
            return BadRequest(new { message = "Employee not found or does not belong to your company." });
        }

        // Отримуємо години за день
        var dailyHours = _context.WorkHours
            .Where(w => w.EmployeeId == employeeId && w.Date.Date == date.Date)
            .AsEnumerable()
            .Select(w => new
            {
                w.Date,
                w.StartTime,
                w.EndTime,
                w.BreakTime,
                w.Urlab,
                w.Krank,
                w.Feiertag,
                w.Baustelle,
                TotalHours = CalculateTotalHours(w.StartTime, w.EndTime, w.BreakTime)
            })
            .ToList();

        return Ok(dailyHours);
    }


    [HttpGet("summary/employee/{employeeId}/month/{month}/{year}")]
    [Authorize]
    public async Task<IActionResult> GetMonthlySummaryForEmployee(int employeeId, int month, int year)
    {
        var companyId = GetCompanyIdFromToken();

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.CompanyId == companyId);

        if (employee == null)
        {
            return BadRequest(new { message = "Employee not found or does not belong to your company." });
        }

        var workHours = _context.WorkHours
            .Where(w => w.EmployeeId == employeeId && w.Date.Month == month && w.Date.Year == year)
            .AsEnumerable()
            .ToList();

        var totalHours = workHours
            .Sum(w => CalculateTotalHours(w.StartTime, w.EndTime, w.BreakTime));

        var salary = totalHours * (double)employee.HourlyRate;

        return Ok(new
        {
            EmployeeId = employeeId,
            TotalHours = totalHours,
            Salary = Math.Round(salary, 2)
        });
    }

    private double CalculateTotalHours(TimeSpan startTime, TimeSpan endTime, int breakTime)
    {
        if (endTime < startTime)
        {
            endTime = endTime.Add(new TimeSpan(24, 0, 0)); // Додаємо 24 години, якщо перехід через опівніч
        }

        return (endTime - startTime).TotalHours - (breakTime / 60.0);
    }

}
