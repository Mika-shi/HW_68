using home_work_66.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Controllers;

[Authorize(Roles = "user,admin,company")]
public class ResumeController : Controller
{
    private readonly HeadHunterContext _db;
    private readonly UserManager<User> _userManager;

    public ResumeController(HeadHunterContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetResumes(int? userId)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        int targetUserId;
        bool isOwner;

        if (userId.HasValue)
        {
            targetUserId = userId.Value;
            isOwner = currentUser != null && currentUser.Id == targetUserId;
        }
        else
        {
            if (currentUser == null)
            {
                return Unauthorized();
            }
            targetUserId = currentUser.Id;
            isOwner = true;
        }

        var query = _db.Resumes
            .Include(r => r.Category)
            .Where(r => r.UserId == targetUserId);

        if (!isOwner)
        {
            query = query.Where(r => r.IsPublished);
        }

        List<Resume> resumes = await query
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();

        ViewBag.IsOwner = isOwner;
        return PartialView("~/Views/Resume/Partials/_ResumesList.cshtml", resumes);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _db.Categories.ToListAsync();
        return PartialView("~/Views/Resume/Partials/_CreateResume.cshtml", new Resume());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Resume resume)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
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

        resume.UserId = currentUser.Id;
        resume.UpdatedAt = DateTime.UtcNow;
        resume.IsPublished = false;

        _db.Resumes.Add(resume);
        await _db.SaveChangesAsync();

        return Json(new
        {
            success = true,
            resumeId = resume.Id,
            redirectUrl = Url.Action("Details", "Resume", new { id = resume.Id })
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Unauthorized();
        }

        Resume? resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

        if (resume == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _db.Categories.ToListAsync();
        return PartialView("~/Views/Resume/Partials/_EditResume.cshtml", resume);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Resume model)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
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

        Resume? resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == model.Id && r.UserId == currentUser.Id);

        if (resume == null)
        {
            return Json(new
            {
                success = false,
                message = "Резюме не найдено"
            });
        }

        resume.Title = model.Title;
        resume.CategoryId = model.CategoryId;
        resume.Salary = model.Salary;

        resume.Email = model.Email;
        resume.PhoneNumber = model.PhoneNumber;
        resume.Telegram = model.Telegram;
        resume.Facebook = model.Facebook;
        resume.LinkedIn = model.LinkedIn;

        await _db.SaveChangesAsync();

        return Json(new
        {
            success = true
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> UpdateDate(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Resume? resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

        if (resume == null)
        {
            return NotFound();
        }

        resume.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

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

        _db.WorkExperiences.Add(workExperience);
        await _db.SaveChangesAsync();

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

        _db.Educations.Add(education);
        await _db.SaveChangesAsync();

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

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = course.ResumeId });
    }
    
    [HttpGet]
    public async Task<IActionResult> EditWorkExperience(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        WorkExperience? workExperience = await _db.WorkExperiences.Include(w => w.Resume).FirstOrDefaultAsync(w => w.Id == id && w.Resume!.UserId == currentUser.Id);

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

        _db.WorkExperiences.Update(workExperience);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = workExperience.ResumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWorkExperience(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        WorkExperience? workExperience = await _db.WorkExperiences.Include(w => w.Resume).FirstOrDefaultAsync(w => w.Id == id && w.Resume!.UserId == currentUser.Id);

        if (workExperience == null)
        {
            return NotFound();
        }
        
        int resumeId = workExperience.ResumeId;

        _db.WorkExperiences.Remove(workExperience);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = resumeId });
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Resume? resume = await _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Courses)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

        if (resume == null)
        {
            return NotFound();
        }

        return View(resume);
    }
    
    [HttpGet]
    public async Task<IActionResult> EditEducation(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Education? education = await _db.Educations.Include(e => e.Resume).FirstOrDefaultAsync(e => e.Id == id && e.Resume!.UserId == currentUser.Id);
        
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
            ModelState.AddModelError(
                "GraduationYear",
                $"Год должен быть от 1900 до {currentYear}"
            );
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Resume/Education/EditEducation.cshtml", education);
        }

        _db.Educations.Update(education);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = education.ResumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEducation(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Education? education = await _db.Educations.Include(e => e.Resume).FirstOrDefaultAsync(e => e.Id == id && e.Resume!.UserId == currentUser.Id);
        
        if (education == null)
        {
            return NotFound();
        }

        int resumeId = education.ResumeId;

        _db.Educations.Remove(education);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = resumeId });
    }
    
    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Course? course = await _db.Courses.Include(c => c.Resume).FirstOrDefaultAsync(c => c.Id == id && c.Resume!.UserId == currentUser.Id);

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

        _db.Courses.Update(course);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = course.ResumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Course? course = await _db.Courses.Include(c => c.Resume).FirstOrDefaultAsync(c => c.Id == id && c.Resume!.UserId == currentUser.Id);

        if (course == null)
        {
            return NotFound();
        }
        
        if (course == null)
        {
            return NotFound();
        }

        int resumeId = course.ResumeId;

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = resumeId });
    }
    
    [HttpPost]
    public async Task<IActionResult> TogglePublication(int id)
    {
        User? currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        Resume? resume = await _db.Resumes
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == currentUser.Id);

        if (resume == null)
        {
            return NotFound();
        }

        resume.IsPublished = !resume.IsPublished;

        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Published()
    {
        List<Resume> resumes = await _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.User)
            .Where(r => r.IsPublished)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();

        return PartialView("Partials/_PublishedResumesPartialView", resumes);
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> PublicDetails(int id)
    {
        Resume? resume = await _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Courses)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsPublished);

        if (resume == null)
        {
            return NotFound();
        }
        
        if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("company"))
        {
            User? currentCompany = await _userManager.GetUserAsync(User);

            if (currentCompany != null)
            {
                ViewBag.Vacancies = await _db.Vacancies.Where(v => v.CompanyId == currentCompany.Id && v.IsPublished).OrderByDescending(v => v.CreatedAt).ToListAsync();
            }
        }

        return View(resume);
    }

    [HttpGet]
    [Authorize(Roles = "company")]
    public async Task<IActionResult> BrowseByCategory(int? categoryId)
    {
        ViewBag.Categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        
        ViewBag.SelectedCategoryId = categoryId;
        var query = _db.Resumes
            .Include(r => r.Category)
            .Include(r => r.User)
            .Where(r => r.IsPublished);

        if (categoryId.HasValue)
        {
            query=query.Where(r=>r.CategoryId==categoryId.Value);
        }

        var rezumes = await query.OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();
        return View(rezumes);
    }
}