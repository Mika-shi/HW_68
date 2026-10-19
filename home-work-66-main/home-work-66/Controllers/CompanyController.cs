using home_work_66.Enums;
using home_work_66.Models;
using home_work_66.Services;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

public class CompanyController : Controller
{
    private readonly HeadHunterContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHeadHunterService _service;

    public CompanyController(HeadHunterContext context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment, IHeadHunterService service)
    {
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
        _service = service;
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
            if (!await _userManager.IsInRoleAsync(currentUser, "company"))
            {
                return Forbid();
            }
            id = currentUser.Id;
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        bool isOwner = currentUser != null && currentUser.Id == user.Id;
        ViewBag.IsOwner = isOwner;

        return View(user);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetVacancies(string? searchString, int? categoryId, VacancySortState sortOrder = VacancySortState.DateDesc, int page = 1)
    {
        VacancyIndexViewModel model = await _service.GetVacanciesAsync(searchString, categoryId, sortOrder, page);

        ViewBag.SalarySort = sortOrder == VacancySortState.SalaryAsc ? VacancySortState.SalaryDesc : VacancySortState.SalaryAsc;
        ViewBag.TitleSort = sortOrder == VacancySortState.TitleAsc ? VacancySortState.TitleDesc : VacancySortState.TitleAsc;
        ViewBag.SortOrder = sortOrder;
        ViewBag.Categories = await _service.GetCategoriesAsync();
        ViewBag.SearchString = searchString;
        ViewBag.CategoryId = categoryId;

        return PartialView("~/Views/Company/Partials/_PublishedVacanciesPartialView.cshtml", model);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCompanyVacancies(int? companyId)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        int targetCompanyId;
        bool isOwner;

        if (companyId.HasValue)
        {
            targetCompanyId = companyId.Value;
            isOwner = currentUser != null && currentUser.Id == targetCompanyId;
        }
        else
        {
            if (currentUser == null)
            {
                return Unauthorized();
            }
            targetCompanyId = currentUser.Id;
            isOwner = true;
        }

        List<Vacancy> vacancies = await _service.GetCompanyVacanciesAsync(targetCompanyId, isOwner);

        ViewBag.IsOwner = isOwner;
        return PartialView("~/Views/Company/Partials/_VacanciesList.cshtml", vacancies);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var vacancy = await _service.GetVacancyByIdAsync(id);

        if (vacancy == null)
            return NotFound();

        return View(vacancy);
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> CreateVacancy()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return PartialView("~/Views/Company/Partials/_CreateVacancy.cshtml", new CreateVacancyViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> CreateVacancy(CreateVacancyViewModel model)
    {
        if (model.ExperienceFrom > model.ExperienceTo)
        {
            ModelState.AddModelError("ExperienceFrom", "Минимальный опыт не может быть больше максимального");
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

        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

        await _service.CreateVacancyAsync(model, user.Id);

        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditVacancy(int id)
    {
        User? user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        CreateVacancyViewModel? model = await _service.GetVacancyForEditAsync(id, user.Id);

        if (model == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();

        return PartialView("~/Views/Company/Partials/_EditVacancy.cshtml", model);
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditVacancy(int id, CreateVacancyViewModel model)
    {
        if (model.ExperienceFrom > model.ExperienceTo)
        {
            ModelState.AddModelError("ExperienceTo", "Максимальный опыт не может быть меньше минимального");
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

        User? user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Json(new { success = false, message = "Пользователь не найден" });
        }

        bool result = await _service.EditVacancyAsync(id, user.Id, model);

        if (!result)
        {
            return Json(new { success = false, message = "Вакансия не найдена" });
        }

        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> UpdateVacancy(int id)
    {
        User? user = await _userManager.GetUserAsync(User);        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

        bool result = await _service.UpdateVacancyAsync(id, user.Id);

        if (!result)
        {
            return Json(new { success = false, message = "Вакансия не найдена" });
        }
        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> TogglePublish(int id)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

        bool result = await _service.TogglePublishAsync(id, user.Id);

        if (!result)
        {
            return Json(new { success = false, message = "Вакансия не найдена" });
        }
        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var model = new EditCompanyProfileViewModel()
        {
            CompanyName = user.CompanyName ?? user.UserName ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            Email = user.Email ?? "",
            AvatarUrl = user.AvatarUrl
        };
        return PartialView("~/Views/Company/Partials/_EditProfile.cshtml", model);
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditProfile(EditCompanyProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

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

        user.CompanyName = model.CompanyName;
        user.PhoneNumber = model.PhoneNumber;
        user.Email = model.Email;

        if (model.Avatar != null && model.Avatar.Length > 0)
        {
            var extension = Path.GetExtension(model.Avatar.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".jfif" };

            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = $"Разрешены только форматы {string.Join(", ", allowedExtensions)}" });
            }

            var wwwPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(wwwPath, "uploads", "avatars");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var newFileName = $"{Guid.NewGuid()}{extension}";
            user.AvatarUrl = $"/uploads/avatars/{newFileName}";
            var fileWithPath = Path.Combine(path, newFileName);
            using var fileStream = new FileStream(fileWithPath, FileMode.Create);
            await model.Avatar.CopyToAsync(fileStream);
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            string message = string.Join("\n", result.Errors.Select(e => e.Description));
            return Json(new { success = false, message });
        }

        return Json(new
        {
            success = true,
            img = user.AvatarUrl,
            name = user.CompanyName,
            email = user.Email
        });
    }
    
    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> MyResponses()
    {
        User? currentCompany = await _userManager.GetUserAsync(User);

        if (currentCompany == null)
        {
            return RedirectToAction("Login", "Account");
        }

        List<Chat> chats = await _context.Chats
            .Where(c => c.CompanyId == currentCompany.Id)
            .Include(c => c.Vacancy)
            .Include(c => c.User)
            .Include(c => c.Resume)
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        List<CompanyResponsesViewModel> responses = chats
            .GroupBy(c => c.VacancyId)
            .Select(group => new CompanyResponsesViewModel
            {
                Vacancy = group.First().Vacancy!,
                Responses = group.ToList()
            })
            .ToList();

        return View(responses);
    }
}