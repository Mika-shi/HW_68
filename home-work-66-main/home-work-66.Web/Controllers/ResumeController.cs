using System.Security.Claims;
using home_work_66.BLL.Models;
using home_work_66.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.Controllers;

[Authorize(Roles = "user,admin,company")]
public class ResumeController : Controller
{
    private readonly IHeadHunterService _service;

    public ResumeController(IHeadHunterService service)
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
    public async Task<IActionResult> GetResumes(int? userId)
    {
        int? currentUserId = GetCurrentUserId();

        int targetUserId;
        bool isOwner;

        if (userId.HasValue)
        {
            targetUserId = userId.Value;
            isOwner = currentUserId != null && currentUserId.Value == targetUserId;
        }
        else
        {
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            targetUserId = currentUserId.Value;
            isOwner = true;
        }

        List<Resume> resumes = await _service.GetResumesAsync(targetUserId, isOwner);

        ViewBag.IsOwner = isOwner;

        return PartialView("~/Views/Resume/Partials/_ResumesList.cshtml", resumes);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _service.GetCategoriesAsync();

        return PartialView("~/Views/Resume/Partials/_CreateResume.cshtml", new Resume());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Resume resume)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Json(new
            {
                success = false,
                message = "Пользователь не найден"
            });
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

        int resumeId = await _service.CreateResumeAsync(resume, userId.Value);

        return Json(new
        {
            success = true,
            resumeId,
            redirectUrl = Url.Action("Details", "Resume", new { id = resumeId })
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Resume? resume = await _service.GetResumeForEditAsync(id, userId.Value);

        if (resume == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _service.GetCategoriesAsync();

        return PartialView("~/Views/Resume/Partials/_EditResume.cshtml", resume);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Resume model)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Json(new
            {
                success = false,
                message = "Пользователь не найден"
            });
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

        bool result = await _service.EditResumeAsync(model, userId.Value);

        if (!result)
        {
            return Json(new
            {
                success = false,
                message = "Резюме не найдено"
            });
        }

        return Json(new
        {
            success = true
        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDate(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        bool result = await _service.UpdateResumeDateAsync(id, userId.Value);

        if (!result)
        {
            return NotFound();
        }

        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult AddWorkExperience(int resumeId)
    {
        ViewBag.ResumeId = resumeId;

        return View("~/Views/Resume/WorkExperience/AddWorkExperience.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkExperience(WorkExperience workExperience)
    {
        int currentYear = DateTime.Now.Year;

        if (workExperience.StartYear < 1900 || workExperience.StartYear > currentYear)
        {
            ModelState.AddModelError("StartYear", $"Год должен быть от 1900 до {currentYear}");
        }

        if (workExperience.EndYear < 1900 || workExperience.EndYear > currentYear)
        {
            ModelState.AddModelError("EndYear", $"Год должен быть от 1900 до {currentYear}");
        }

        if (workExperience.EndYear < workExperience.StartYear)
        {
            ModelState.AddModelError("EndYear", "Год окончания не может быть раньше года начала");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = workExperience.ResumeId;

            return View("~/Views/Resume/WorkExperience/AddWorkExperience.cshtml", workExperience);
        }

        await _service.AddWorkExperienceAsync(workExperience);

        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult AddEducation(int resumeId)
    {
        ViewBag.ResumeId = resumeId;

        return View("~/Views/Resume/Education/AddEducation.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEducation(Education education)
    {
        int currentYear = DateTime.Now.Year;

        if (education.GraduationYear < 1900 || education.GraduationYear > currentYear)
        {
            ModelState.AddModelError("GraduationYear", $"Год должен быть от 1900 до {currentYear}");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = education.ResumeId;

            return View("~/Views/Resume/Education/AddEducation.cshtml", education);
        }

        await _service.AddEducationAsync(education);

        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult AddCourse(int resumeId)
    {
        ViewBag.ResumeId = resumeId;

        return View("~/Views/Resume/Courses/AddCourse.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCourse(Course course)
    {
        int currentYear = DateTime.Now.Year;

        if (course.Year < 1900 || course.Year > currentYear)
        {
            ModelState.AddModelError("Year", $"Год должен быть от 1900 до {currentYear}");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = course.ResumeId;

            return View("~/Views/Resume/Courses/AddCourse.cshtml", course);
        }

        await _service.AddCourseAsync(course);

        return RedirectToAction("Details", new { id = course.ResumeId });
    }

    [HttpGet]
    public async Task<IActionResult> EditWorkExperience(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        WorkExperience? workExperience = await _service.GetWorkExperienceAsync(id, userId.Value);

        if (workExperience == null)
        {
            return NotFound();
        }

        return View("~/Views/Resume/WorkExperience/EditWorkExperience.cshtml", workExperience);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWorkExperience(WorkExperience workExperience)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Resume/WorkExperience/EditWorkExperience.cshtml", workExperience);
        }

        await _service.UpdateWorkExperienceAsync(workExperience);

        return RedirectToAction("Details", new { id = workExperience.ResumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWorkExperience(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int? resumeId = await _service.DeleteWorkExperienceAsync(id, userId.Value);

        if (resumeId == null)
        {
            return NotFound();
        }

        return RedirectToAction("Details", new { id = resumeId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Resume? resume = await _service.GetResumeDetailsAsync(id, userId.Value);

        if (resume == null)
        {
            return NotFound();
        }

        return View(resume);
    }

    [HttpGet]
    public async Task<IActionResult> EditEducation(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Education? education = await _service.GetEducationAsync(id, userId.Value);

        if (education == null)
        {
            return NotFound();
        }

        return View("~/Views/Resume/Education/EditEducation.cshtml", education);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEducation(Education education)
    {
        int currentYear = DateTime.Now.Year;

        if (education.GraduationYear < 1900 || education.GraduationYear > currentYear)
        {
            ModelState.AddModelError("GraduationYear", $"Год должен быть от 1900 до {currentYear}");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Resume/Education/EditEducation.cshtml", education);
        }

        await _service.UpdateEducationAsync(education);

        return RedirectToAction("Details", new { id = education.ResumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEducation(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int? resumeId = await _service.DeleteEducationAsync(id, userId.Value);

        if (resumeId == null)
        {
            return NotFound();
        }

        return RedirectToAction("Details", new { id = resumeId.Value });
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Course? course = await _service.GetCourseAsync(id, userId.Value);

        if (course == null)
        {
            return NotFound();
        }

        return View("~/Views/Resume/Courses/EditCourse.cshtml", course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(Course course)
    {
        int currentYear = DateTime.Now.Year;

        if (course.Year < 1900 || course.Year > currentYear)
        {
            ModelState.AddModelError("Year", $"Год должен быть от 1900 до {currentYear}");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Resume/Courses/EditCourse.cshtml", course);
        }

        await _service.UpdateCourseAsync(course);

        return RedirectToAction("Details", new { id = course.ResumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int? resumeId = await _service.DeleteCourseAsync(id, userId.Value);

        if (resumeId == null)
        {
            return NotFound();
        }

        return RedirectToAction("Details", new { id = resumeId.Value });
    }

    [HttpPost]
    public async Task<IActionResult> TogglePublication(int id)
    {
        int? userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        bool result = await _service.ToggleResumePublicationAsync(id, userId.Value);

        if (!result)
        {
            return NotFound();
        }

        return Json(new { success = true });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Published()
    {
        List<Resume> resumes = await _service.GetPublishedResumesAsync();

        return PartialView("Partials/_PublishedResumesPartialView", resumes);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> PublicDetails(int id)
    {
        Resume? resume = await _service.GetPublicResumeDetailsAsync(id);

        if (resume == null)
        {
            return NotFound();
        }

        if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("company"))
        {
            int? companyId = GetCurrentUserId();

            if (companyId != null)
            {
                ViewBag.Vacancies = await _service.GetCompanyVacanciesAsync(companyId.Value, false);
            }
        }

        return View(resume);
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> BrowseByCategory(int? categoryId)
    {
        ViewBag.Categories = await _service.GetCategoriesOrderedAsync();
        ViewBag.SelectedCategoryId = categoryId;

        List<Resume> resumes = await _service.GetPublishedResumesByCategoryAsync(categoryId);

        return View(resumes);
    }
}