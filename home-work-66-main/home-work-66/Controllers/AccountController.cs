using home_work_66.Models;
using home_work_66.Services;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

public class AccountController : Controller
{
    private readonly IHeadHunterService _service;

    public AccountController(IHeadHunterService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        AccountResult result = await _service.RegisterAsync(model);

        if (!result.Succeeded)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        if (result.IsCompany)
        {
            return RedirectToAction("Index", "Company");
        }

        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        AccountResult result = await _service.LoginAsync(model);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message ?? "Неправильный логин и (или) пароль");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        if (result.IsCompany)
        {
            return RedirectToAction("Index", "Company");
        }

        return RedirectToAction("Index", "User");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOffAsync()
    {
        await _service.LogOffAsync();

        return RedirectToAction("Login");
    }
}