using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrekingTIme.DTO.Company;
using TrekingTIme.Models;
using TrekingTIme;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCompanyDto dto)
    {
     
        if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
        {
            return BadRequest(new { message = "A company with this email already exists." });
        }

        var company = new Company
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) 
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Company registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCompanyDto dto)
    {
        //  поштa
        var company = await _context.Companies.SingleOrDefaultAsync(c => c.Email == dto.Email);
        if (company == null || !BCrypt.Net.BCrypt.Verify(dto.Password, company.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = GenerateJwtToken(company);
        return Ok(new { token });
    }

    private string GenerateJwtToken(Company company)
    {
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, company.CompanyId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, company.Email),
        new Claim("CompanyName", company.Name)
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
