using System.Security.Claims;
using home_work_66.BLL.DTOs;
using home_work_66.BLL.Models;
using home_work_66.BLL.Enums;
using home_work_66.Services;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

public class CompanyController : Controller
{
    private readonly IHeadHunterService _service;

    public CompanyController(IHeadHunterService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(int? id)
    {
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = int.TryParse(currentUserIdString, out int parsedId) ? parsedId : null;
        if (id == null)
        {
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (!User.IsInRole("company"))
            {
                return Forbid();
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
    [AllowAnonymous]
    public async Task<IActionResult> GetVacancies(string? searchString, int? categoryId, VacancySortState sortOrder = VacancySortState.DateDesc, int page = 1)
    {
        VacancyListDto result = await _service.GetVacanciesAsync(searchString, categoryId, sortOrder, page);

        VacancyIndexViewModel model = new VacancyIndexViewModel
        {
            Vacancies = result.Vacancies.Select(v => new Vacancy
            {
                Id = v.Id,
                Title = v.Title,
                Salary = v.Salary,
                Description = v.Description,
                Requirements = v.Requirements,
                ExperienceFrom = v.ExperienceFrom,
                ExperienceTo = v.ExperienceTo,
                CategoryId = v.CategoryId,
                CompanyId = v.CompanyId,
                IsPublished = v.IsPublished,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                Category = v.Category,
                Company = v.Company
            }).ToList(),

            PageViewModel = new PageViewModel(
                result.Vacancies.Count,
                result.CurrentPage,
                20
            )
        };

        return View(model);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCompanyVacancies(int? companyId)
    {
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = int.TryParse(currentUserIdString, out int parsedId) ? parsedId : null;

        int targetCompanyId;
        bool isOwner;

        if (companyId.HasValue)
        {
            targetCompanyId = companyId.Value;
            isOwner = currentUserId != null && currentUserId.Value == targetCompanyId;
        }
        else
        {
            if (currentUserId == null)
            {
                return Unauthorized();
            }
            targetCompanyId = currentUserId.Value;
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
        ViewBag.Categories = await _service.GetCategoriesAsync();
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

        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!int.TryParse(currentUserIdString, out int userId))
            return Json(new { success = false, message = "Пользователь не найден" });

        CreateVacancyDto dto = new CreateVacancyDto
        {
            Id = model.Id,
            Title = model.Title,
            Salary = model.Salary,
            Description = model.Description,
            Requirements = model.Requirements,
            ExperienceFrom = model.ExperienceFrom,
            ExperienceTo = model.ExperienceTo,
            CategoryId = model.CategoryId
        };

        await _service.CreateVacancyAsync(dto, userId);

        return Json(new { success = true });
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditVacancy(int id)
    {
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(currentUserIdString, out int userId))
        {
            return NotFound();
        }

        CreateVacancyDto? dto = await _service.GetVacancyForEditAsync(id, userId);

        if (dto == null)
        {
            return NotFound();
        }

        CreateVacancyViewModel model = new CreateVacancyViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Salary = dto.Salary,
            Description = dto.Description,
            Requirements = dto.Requirements,
            ExperienceFrom = dto.ExperienceFrom,
            ExperienceTo = dto.ExperienceTo,
            CategoryId = dto.CategoryId
        };

        ViewBag.Categories = await _service.GetCategoriesAsync();
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
                message = string.IsNullOrWhiteSpace(message)
                    ? "Пожалуйста, заполните все обязательные поля корректно"
                    : message
            });
        }

        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(currentUserIdString, out int userId))
        {
            return Json(new
            {
                success = false,
                message = "Пользователь не найден"
            });
        }

        CreateVacancyDto dto = new CreateVacancyDto
        {
            Id = model.Id,
            Title = model.Title,
            Salary = model.Salary,
            Description = model.Description,
            Requirements = model.Requirements,
            ExperienceFrom = model.ExperienceFrom,
            ExperienceTo = model.ExperienceTo,
            CategoryId = model.CategoryId
        };

        bool result = await _service.EditVacancyAsync(id, userId, dto);

        if (!result)
        {
            return Json(new
            {
                success = false,
                message = "Вакансия не найдена"
            });
        }

        return Json(new
        {
            success = true
        });
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> UpdateVacancy(int id)
    {
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(currentUserIdString, out int userId))
        {
            return Json(new { success = false, message = "Пользователь не найден" });
        }

        bool result = await _service.UpdateVacancyAsync(id, userId);

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
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(currentUserIdString, out int userId))
        {
            return Json(new { success = false, message = "Пользователь не найден" });
        }

        bool result = await _service.TogglePublishAsync(id, userId);

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
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(currentUserIdString, out int userId))
        {
            return NotFound();
        }

        EditCompanyProfileViewModel? model = await _service.GetCompanyProfileForEditAsync(userId);

        if (model == null)
        {
            return NotFound();
        }
        return PartialView("~/Views/Company/Partials/_EditProfile.cshtml", model);
    }

    [HttpPost]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> EditProfile(EditCompanyProfileViewModel model)
    {
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

        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(currentUserIdString, out int userId))
        {
            return Json(new { success = false, message = "Пользователь не найден" });
        }

        EditCompanyProfileResult result = await _service.UpdateCompanyProfileAsync(userId, model);

        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.Message });
        }

        return Json(new
        {
            success = true,
            img = result.AvatarUrl,
            name = result.CompanyName,
            email = result.Email
        });
    }
    

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> MyResponses()
    {
        string? currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(currentUserIdString, out int userId))
        {
            return RedirectToAction("Login", "Account");
        }

        List<CompanyResponsesDto> result = await _service.GetCompanyResponsesAsync(userId);

        List<CompanyResponsesViewModel> responses = result
            .Select(x => new CompanyResponsesViewModel
            {
                Vacancy = x.Vacancy,
                Responses = x.Responses
            })
            .ToList();

        return View(responses);
    }
}