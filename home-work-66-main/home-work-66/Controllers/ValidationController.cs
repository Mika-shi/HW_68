using home_work_66.Models;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

public class ValidationController : Controller
{
    private readonly HeadHunterContext _context;
    public ValidationController(HeadHunterContext context)
    {
        _context = context;
    }
    [AcceptVerbs("GET", "POST")]
    public async Task<bool> CheckEmail(string? email, int? id)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true;
        if(id.HasValue)
            return !_context.Users.Any(u => u.Email == email &&  u.Id != id.Value);
        return !_context.Users.Any(u => u.Email == email);
    }
    [AcceptVerbs("GET", "POST")]
    
    public bool CheckPassword(string? password)
    {
        if(string.IsNullOrWhiteSpace(password))
            return true;
        return (password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit));
    }
}