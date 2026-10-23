using home_work_66.BLL.DTOs;
using home_work_66.BLL.Interfaces;
using home_work_66.BLL.Models;
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

        string avatarUrl = "/default.jpg";

        if (model.Avatar != null && model.Avatar.Length > 0)
        {
            string uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "avatars"
            );

            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Avatar.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            await model.Avatar.CopyToAsync(fileStream);

            avatarUrl = $"/uploads/avatars/{uniqueFileName}";
        }

        RegisterDto dto = new RegisterDto
        {
            Email = model.Email,
            UserName = model.UserName,
            PhoneNumber = model.PhoneNumber,
            Password = model.Password,
            IsCompany = model.IsCompany,
            AvatarUrl = avatarUrl
        };

        AccountResultDto result = await _service.RegisterAsync(dto);

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

        LoginDto dto = new LoginDto
        {
            Email = model.Email,
            Password = model.Password,
            RememberMe = model.RememberMe
        };

        AccountResultDto result = await _service.LoginAsync(dto);

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