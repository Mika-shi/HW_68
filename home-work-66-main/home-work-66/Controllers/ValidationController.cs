using home_work_66.Services;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

public class ValidationController : Controller
{
    private readonly IHeadHunterService _service;

    public ValidationController(IHeadHunterService service)
    {
        _service = service;
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<bool> CheckEmail(string? email, int? id)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return true;
        }

        return await _service.IsEmailAvailableAsync(email, id);
    }

    [AcceptVerbs("GET", "POST")]
    public bool CheckPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return true;
        }

        return _service.IsPasswordValid(password);
    }
}