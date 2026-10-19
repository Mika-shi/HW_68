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
    public async Task<IActionResult> GetVacancies(string? searchString, int? categoryId,VacancySortState sortOrder=VacancySortState.DateDesc, int page=1)
    {
        var query = _context.Vacancies
            .Include(v => v.Category)
            .Include(v => v.Company)
            .Where(v => v.IsPublished);
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            query = query.Where(v => v.Title.Contains(searchString));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(v => v.CategoryId == categoryId.Value);
        }

        switch (sortOrder)
        {
            case VacancySortState.SalaryAsc: 
                query = query.OrderBy(v => v.Salary); 
                break;
            case VacancySortState.SalaryDesc:
                query = query.OrderByDescending(v => v.Salary);
                break;
            case VacancySortState.TitleAsc:
                query = query.OrderBy(v => v.Title);
                break;
            case VacancySortState.TitleDesc:
                query = query.OrderByDescending(v => v.Title);
                break;
            case VacancySortState.DateDesc:
            default:
                query = query.OrderByDescending(v => v.UpdatedAt ?? v.CreatedAt);
                break;
        }
         ViewBag.SalarySort = sortOrder == VacancySortState.SalaryAsc ? VacancySortState.SalaryDesc : VacancySortState.SalaryAsc;
         ViewBag.TitleSort = sortOrder == VacancySortState.TitleAsc ? VacancySortState.TitleDesc : VacancySortState.TitleAsc; 
         ViewBag.SortOrder = sortOrder;
         ViewBag.Categories = await _context.Categories.ToListAsync();
         ViewBag.SearchString=searchString;
         ViewBag.CategoryId=categoryId;
        
         int pageSize = 20;
         var count= await query.CountAsync();
         var items= await query
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync();
          
        PageViewModel pvm = new PageViewModel(count, page, pageSize);
        VacancyIndexViewModel vivm = new VacancyIndexViewModel()
        {
             Vacancies = items,
             PageViewModel = pvm
        };
   
        return PartialView( "~/Views/Company/Partials/_PublishedVacanciesPartialView.cshtml", vivm);
        
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

        var query = _context.Vacancies
            .Include(v => v.Category)
            .Where(v => v.CompanyId == targetCompanyId);

        if (!isOwner)
        {
            query = query.Where(v => v.IsPublished);
        }

        var vacancies = await query
            .OrderByDescending(v => v.UpdatedAt)
            .ToListAsync();

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

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

        var vacancy = new Vacancy
        {
            Title = model.Title,
            Salary = model.Salary,
            Description = model.Description,
            Requirements = model.Requirements,
            ExperienceFrom = model.ExperienceFrom,
            ExperienceTo = model.ExperienceTo,
            CategoryId = model.CategoryId,
            CompanyId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublished = false
        };
        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditVacancy(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == user.Id);

        if (vacancy == null)
            return NotFound();

        var model = new CreateVacancyViewModel
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            Salary = vacancy.Salary,
            Description = vacancy.Description,
            Requirements = vacancy.Requirements,
            ExperienceFrom = vacancy.ExperienceFrom,
            ExperienceTo = vacancy.ExperienceTo,
            CategoryId = vacancy.CategoryId
        };

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

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == user.Id);

        if (vacancy == null)
            return Json(new { success = false, message = "Вакансия не найдена" });

        vacancy.Title = model.Title;
        vacancy.Salary = model.Salary;
        vacancy.Description = model.Description;
        vacancy.Requirements = model.Requirements;
        vacancy.ExperienceFrom = model.ExperienceFrom;
        vacancy.ExperienceTo = model.ExperienceTo;
        vacancy.CategoryId = model.CategoryId;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> UpdateVacancy(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == user.Id);

        if (vacancy == null)
            return Json(new { success = false, message = "Вакансия не найдена" });

        vacancy.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> TogglePublish(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден" });

        var vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == user.Id);
        if (vacancy == null)
            return Json(new { success = false, message = "Вакансия не найдена" });

        vacancy.IsPublished = !vacancy.IsPublished;
        vacancy.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
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