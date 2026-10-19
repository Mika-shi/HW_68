using home_work_66.Models;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace home_work_66.Controllers;

public class UserController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly HeadHunterContext _db;

    public UserController(UserManager<User> userManager, IWebHostEnvironment environment, HeadHunterContext db)
    {
        _userManager = userManager;
        _environment = environment;
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(int? id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (id == null)
        {
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            id = currentUser.Id;
        }

        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        bool isOwner = currentUser != null && currentUser.Id == user.Id;
        ViewBag.IsOwner = isOwner;

        return View(user);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        EditProfileViewModel model = new EditProfileViewModel
        {
            UserName = currentUser.UserName,
            Email = currentUser.Email,
            PhoneNumber = currentUser.PhoneNumber
        };

        return PartialView("_EditProfilePartialView", model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
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

        currentUser.UserName = model.UserName;
        currentUser.Email = model.Email;
        currentUser.PhoneNumber = model.PhoneNumber;

        if (model.Avatar != null && model.Avatar.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Avatar.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            await model.Avatar.CopyToAsync(fileStream);

            currentUser.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
        }

        IdentityResult result = await _userManager.UpdateAsync(currentUser);

        if (!result.Succeeded)
        {
            string message = string.Join("\n", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message });
        }

        return Json(new
        {
            success = true,
            img = currentUser.AvatarUrl,
            name = currentUser.UserName,
            email = currentUser.Email
        });
    }
    [Authorize]
    public async Task<IActionResult> Respond(int vacancyId, int companyId)
    {
        int userId = int.Parse(_userManager.GetUserId(User)!);
        var resumes = await _db.Resumes
            .Where(x => x.UserId == userId)
            .Include(x => x.Category)
            .ToListAsync();

        var model = new ChatCreateViewModel
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
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        List<Chat> responses = await _db.Chats.Where(c => c.UserId == currentUser.Id).Include(c => c.Vacancy).Include(c => c.Company).Include(c => c.Resume).ToListAsync();

        return View(responses);
    }
}