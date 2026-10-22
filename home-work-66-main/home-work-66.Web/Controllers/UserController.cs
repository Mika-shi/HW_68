using System.Security.Claims;
using home_work_66.BLL.Models;
using home_work_66.Services;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace home_work_66.Controllers;

public class UserController : Controller
{
    private readonly IHeadHunterService _service;

    public UserController(IHeadHunterService service)
    {
        _service = service;
    }

    private int? GetCurrentUserId()
    {
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserIdString, out int userId) ? userId : null;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(int? id)
    {
        int? currentUserId = GetCurrentUserId();

        if (id == null)
        {
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            id = currentUserId.Value;
        }

        User? user = await _service.GetUserByIdAsync(id.Value);

        if (user == null)
        {
            return NotFound();
        }

        bool isOwner = currentUserId != null && currentUserId.Value == user.Id;
        ViewBag.IsOwner = isOwner;

        return View(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        EditProfileViewModel? model = await _service.GetUserProfileForEditAsync(userId.Value);

        if (model == null)
        {
            return Unauthorized();
        }

        return PartialView("_EditProfilePartialView", model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Json(new { success = false, message = "Пользователь не найден" });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => e.ErrorMessage);

            string message = string.Join("\n", errors);

            return Json(new
            {
                success = false,
                message = string.IsNullOrWhiteSpace(message) ? "Пожалуйста, заполните все обязательные поля корректно" : message
            });
        }

        EditUserProfileResult result = await _service.UpdateUserProfileAsync(userId.Value, model);

        if (!result.Succeeded)
        {
            return Json(new
            {
                success = false,
                message = result.Message
            });
        }

        return Json(new
        {
            success = true,
            img = result.AvatarUrl,
            name = result.UserName,
            email = result.Email
        });
    }

    [Authorize]
    public async Task<IActionResult> Respond(int vacancyId, int companyId)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        List<Resume> resumes = await _service.GetUserResumesAsync(userId.Value);

        ChatCreateViewModel model = new ChatCreateViewModel
        {
            VacancyId = vacancyId,
            CompanyId = companyId,
            Resumes = resumes
        };

        return PartialView("_RespondPartialView", model);
    }

    [HttpGet]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> MyResponses()
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        List<Chat> responses = await _service.GetUserResponsesAsync(userId.Value);

        return View(responses);
    }
}