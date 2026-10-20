using home_work_66.Enums;
using home_work_66.Models;
using home_work_66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Services;

public class HeadHunterService : IHeadHunterService
{
    private readonly HeadHunterContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HeadHunterService(HeadHunterContext context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<Vacancy?> GetVacancyByIdAsync(int id)
    {
        return await _context.Vacancies
            .Include(v => v.Category)
            .Include(v => v.Company)
            .FirstOrDefaultAsync(v => v.Id == id);
    }
    
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }
    
    public async Task<VacancyIndexViewModel> GetVacanciesAsync(string? searchString, int? categoryId, VacancySortState sortOrder, int page)
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

        int pageSize = 20;
        int count = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        PageViewModel pageViewModel = new PageViewModel(count, page, pageSize);

        return new VacancyIndexViewModel
        {
            Vacancies = items,
            PageViewModel = pageViewModel
        };
    }
    
    public async Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId, bool isOwner)
    {
        var query = _context.Vacancies
            .Include(v => v.Category)
            .Where(v => v.CompanyId == companyId);

        if (!isOwner)
        {
            query = query.Where(v => v.IsPublished);
        }

        return await query
            .OrderByDescending(v => v.UpdatedAt)
            .ToListAsync();
    }
    
    public async Task<bool> CreateVacancyAsync(CreateVacancyViewModel model, int companyId)
    {
        Vacancy vacancy = new Vacancy
        {
            Title = model.Title,
            Salary = model.Salary,
            Description = model.Description,
            Requirements = model.Requirements,
            ExperienceFrom = model.ExperienceFrom,
            ExperienceTo = model.ExperienceTo,
            CategoryId = model.CategoryId,
            CompanyId = companyId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublished = false
        };

        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<CreateVacancyViewModel?> GetVacancyForEditAsync(int id, int companyId)
    {
        Vacancy? vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);

        if (vacancy == null)
        {
            return null;
        }

        return new CreateVacancyViewModel
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
    }
    
    public async Task<bool> EditVacancyAsync(int id, int companyId, CreateVacancyViewModel model)
    {
        Vacancy? vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);

        if (vacancy == null)
        {
            return false;
        }

        vacancy.Title = model.Title;
        vacancy.Salary = model.Salary;
        vacancy.Description = model.Description;
        vacancy.Requirements = model.Requirements;
        vacancy.ExperienceFrom = model.ExperienceFrom;
        vacancy.ExperienceTo = model.ExperienceTo;
        vacancy.CategoryId = model.CategoryId;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> UpdateVacancyAsync(int id, int companyId)
    {
        Vacancy? vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);

        if (vacancy == null)
        {
            return false;
        }

        vacancy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> TogglePublishAsync(int id, int companyId)
    {
        Vacancy? vacancy = await _context.Vacancies.FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);

        if (vacancy == null)
        {
            return false;
        }

        vacancy.IsPublished = !vacancy.IsPublished;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<List<CompanyResponsesViewModel>> GetCompanyResponsesAsync(int companyId)
    {
        List<Chat> chats = await _context.Chats
            .Where(c => c.CompanyId == companyId)
            .Include(c => c.Vacancy)
            .Include(c => c.User)
            .Include(c => c.Resume)
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        return chats
            .GroupBy(c => c.VacancyId)
            .Select(group => new CompanyResponsesViewModel
            {
                Vacancy = group.First().Vacancy!,
                Responses = group.ToList()
            })
            .ToList();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<EditCompanyProfileViewModel?> GetCompanyProfileForEditAsync(int userId)
    {
        User? user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return null;
        }

        return new EditCompanyProfileViewModel
        {
            CompanyName = user.CompanyName ?? user.UserName ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            Email = user.Email ?? "",
            AvatarUrl = user.AvatarUrl
        };
    }
    
    public async Task<EditCompanyProfileResult> UpdateCompanyProfileAsync(int userId, EditCompanyProfileViewModel model)
    {
        User? user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return new EditCompanyProfileResult { Succeeded = false, Message = "Пользователь не найден" };
        }

        user.CompanyName = model.CompanyName;
        user.PhoneNumber = model.PhoneNumber;
        user.Email = model.Email;

        if (model.Avatar != null && model.Avatar.Length > 0)
        {
            string extension = Path.GetExtension(model.Avatar.FileName).ToLower();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".jfif" };

            if (!allowedExtensions.Contains(extension))
            {
                return new EditCompanyProfileResult
                {
                    Succeeded  = false,
                    Message = $"Разрешены только форматы {string.Join(", ", allowedExtensions)}"
                };
            }

            string path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string newFileName = $"{Guid.NewGuid()}{extension}";
            user.AvatarUrl = $"/uploads/avatars/{newFileName}";

            string fileWithPath = Path.Combine(path, newFileName);

            using FileStream fileStream = new FileStream(fileWithPath, FileMode.Create);
            await model.Avatar.CopyToAsync(fileStream);
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return new EditCompanyProfileResult
            {
                Succeeded = false,
                Message = string.Join("\n", result.Errors.Select(e => e.Description))
            };
        }

        return new EditCompanyProfileResult
        {
            Succeeded = true,
            AvatarUrl = user.AvatarUrl,
            CompanyName = user.CompanyName,
            Email = user.Email
        };
    }
    
    public async Task<List<Resume>> GetResumesAsync(int userId, bool isOwner)
{
    var query = _context.Resumes
        .Include(r => r.Category)
        .Where(r => r.UserId == userId);

    if (!isOwner)
    {
        query = query.Where(r => r.IsPublished);
    }

    return await query
        .OrderByDescending(r => r.UpdatedAt)
        .ToListAsync();
}

    public async Task<int> CreateResumeAsync(Resume resume, int userId)
{
    resume.UserId = userId;
    resume.UpdatedAt = DateTime.UtcNow;
    resume.IsPublished = false;

    _context.Resumes.Add(resume);
    await _context.SaveChangesAsync();

    return resume.Id;
}

    public async Task<Resume?> GetResumeForEditAsync(int id, int userId)
{
    return await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
}

    public async Task<bool> EditResumeAsync(Resume model, int userId)
{
    Resume? resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == model.Id && r.UserId == userId);

    if (resume == null)
    {
        return false;
    }

    resume.Title = model.Title;
    resume.CategoryId = model.CategoryId;
    resume.Salary = model.Salary;

    resume.Email = model.Email;
    resume.PhoneNumber = model.PhoneNumber;
    resume.Telegram = model.Telegram;
    resume.Facebook = model.Facebook;
    resume.LinkedIn = model.LinkedIn;

    await _context.SaveChangesAsync();

    return true;
}

    public async Task<bool> UpdateResumeDateAsync(int id, int userId)
{
    Resume? resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

    if (resume == null)
    {
        return false;
    }

    resume.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return true;
}

    public async Task AddWorkExperienceAsync(WorkExperience workExperience)
{
    _context.WorkExperiences.Add(workExperience);
    await _context.SaveChangesAsync();
}

    public async Task<WorkExperience?> GetWorkExperienceAsync(int id, int userId)
{
    return await _context.WorkExperiences
        .Include(w => w.Resume)
        .FirstOrDefaultAsync(w => w.Id == id && w.Resume!.UserId == userId);
}

    public async Task UpdateWorkExperienceAsync(WorkExperience workExperience)
{
    _context.WorkExperiences.Update(workExperience);
    await _context.SaveChangesAsync();
}

    public async Task<int?> DeleteWorkExperienceAsync(int id, int userId)
{
    WorkExperience? workExperience = await _context.WorkExperiences
        .Include(w => w.Resume)
        .FirstOrDefaultAsync(w => w.Id == id && w.Resume!.UserId == userId);

    if (workExperience == null)
    {
        return null;
    }

    int resumeId = workExperience.ResumeId;

    _context.WorkExperiences.Remove(workExperience);
    await _context.SaveChangesAsync();

    return resumeId;
}

    public async Task AddEducationAsync(Education education)
{
    _context.Educations.Add(education);
    await _context.SaveChangesAsync();
}

    public async Task<Education?> GetEducationAsync(int id, int userId)
{
    return await _context.Educations
        .Include(e => e.Resume)
        .FirstOrDefaultAsync(e => e.Id == id && e.Resume!.UserId == userId);
}

    public async Task UpdateEducationAsync(Education education)
{
    _context.Educations.Update(education);
    await _context.SaveChangesAsync();
}

    public async Task<int?> DeleteEducationAsync(int id, int userId)
{
    Education? education = await _context.Educations
        .Include(e => e.Resume)
        .FirstOrDefaultAsync(e => e.Id == id && e.Resume!.UserId == userId);

    if (education == null)
    {
        return null;
    }

    int resumeId = education.ResumeId;

    _context.Educations.Remove(education);
    await _context.SaveChangesAsync();

    return resumeId;
}

    public async Task AddCourseAsync(Course course)
{
    _context.Courses.Add(course);
    await _context.SaveChangesAsync();
}

    public async Task<Course?> GetCourseAsync(int id, int userId)
{
    return await _context.Courses
        .Include(c => c.Resume)
        .FirstOrDefaultAsync(c => c.Id == id && c.Resume!.UserId == userId);
}

    public async Task UpdateCourseAsync(Course course)
{
    _context.Courses.Update(course);
    await _context.SaveChangesAsync();
}

    public async Task<int?> DeleteCourseAsync(int id, int userId)
{
    Course? course = await _context.Courses
        .Include(c => c.Resume)
        .FirstOrDefaultAsync(c => c.Id == id && c.Resume!.UserId == userId);

    if (course == null)
    {
        return null;
    }

    int resumeId = course.ResumeId;

    _context.Courses.Remove(course);
    await _context.SaveChangesAsync();

    return resumeId;
}

    public async Task<Resume?> GetResumeDetailsAsync(int id, int userId)
{
    return await _context.Resumes
        .Include(r => r.Category)
        .Include(r => r.WorkExperiences)
        .Include(r => r.Educations)
        .Include(r => r.Courses)
        .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
}

    public async Task<bool> ToggleResumePublicationAsync(int id, int userId)
{
    Resume? resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

    if (resume == null)
    {
        return false;
    }

    resume.IsPublished = !resume.IsPublished;

    await _context.SaveChangesAsync();

    return true;
}

    public async Task<List<Resume>> GetPublishedResumesAsync()
{
    return await _context.Resumes
        .Include(r => r.Category)
        .Include(r => r.User)
        .Where(r => r.IsPublished)
        .OrderByDescending(r => r.UpdatedAt)
        .ToListAsync();
}

    public async Task<Resume?> GetPublicResumeDetailsAsync(int id)
{
    return await _context.Resumes
        .Include(r => r.Category)
        .Include(r => r.WorkExperiences)
        .Include(r => r.Educations)
        .Include(r => r.Courses)
        .FirstOrDefaultAsync(r => r.Id == id && r.IsPublished);
}

    public async Task<List<Category>> GetCategoriesOrderedAsync()
{
    return await _context.Categories
        .OrderBy(c => c.Name)
        .ToListAsync();
}

    public async Task<List<Resume>> GetPublishedResumesByCategoryAsync(int? categoryId)
{
    var query = _context.Resumes
        .Include(r => r.Category)
        .Include(r => r.User)
        .Where(r => r.IsPublished);

    if (categoryId.HasValue)
    {
        query = query.Where(r => r.CategoryId == categoryId.Value);
    }

    return await query
        .OrderByDescending(r => r.UpdatedAt)
        .ToListAsync();
}
    
    public async Task<Chat> CreateChatAsync(ChatCreateViewModel model, int userId)
{
    Chat? existingChat = await _context.Chats
        .FirstOrDefaultAsync(c => c.UserId == userId && c.VacancyId == model.VacancyId);

    if (existingChat != null)
    {
        return existingChat;
    }

    Chat chat = new Chat
    {
        UserId = userId,
        CompanyId = model.CompanyId,
        VacancyId = model.VacancyId,
        ResumeId = model.ResumeId!.Value
    };

    await _context.Chats.AddAsync(chat);
    await _context.SaveChangesAsync();

    return chat;
}

    public async Task<ChatPageViewModel?> GetChatPageAsync(int chatId)
{
    Chat? chat = await _context.Chats
        .Include(x => x.Vacancy)
        .Include(x => x.Resume)
        .Include(x => x.User)
        .Include(x => x.Company)
        .FirstOrDefaultAsync(x => x.Id == chatId);

    if (chat == null)
    {
        return null;
    }

    List<Message> messages = await _context.Messages
        .Where(x => x.ChatId == chatId)
        .Include(x => x.User)
        .OrderBy(x => x.CreatedAt)
        .ToListAsync();

    return new ChatPageViewModel
    {
        Chat = chat,
        Messages = messages
    };
}

    public async Task<List<Message>> GetMessagesAsync(int? chatId)
{
    return await _context.Messages
        .Where(x => x.ChatId == chatId)
        .Include(x => x.User)
        .OrderBy(x => x.CreatedAt)
        .ToListAsync();
}

    public async Task<List<Message>> SendMessageAsync(MessageCreateViewModel model, int userId)
{
    Message message = new Message
    {
        Content = model.Content,
        UserId = userId,
        CreatedAt = DateTime.UtcNow,
        ChatId = model.ChatId
    };

    await _context.Messages.AddAsync(message);
    await _context.SaveChangesAsync();

    return await _context.Messages
        .Where(x => x.ChatId == model.ChatId)
        .Include(x => x.User)
        .OrderBy(x => x.CreatedAt)
        .ToListAsync();
}

    public async Task<Chat?> CreateChatFromResumeAsync(int resumeId, int vacancyId, int companyId)
{
    Resume? resume = await _context.Resumes
        .FirstOrDefaultAsync(r => r.Id == resumeId && r.IsPublished);

    if (resume == null)
    {
        return null;
    }

    Vacancy? vacancy = await _context.Vacancies
        .FirstOrDefaultAsync(v => v.Id == vacancyId && v.CompanyId == companyId);

    if (vacancy == null)
    {
        return null;
    }

    Chat? existingChat = await _context.Chats
        .FirstOrDefaultAsync(c =>
            c.UserId == resume.UserId &&
            c.CompanyId == companyId &&
            c.VacancyId == vacancyId);

    if (existingChat != null)
    {
        return existingChat;
    }

    Chat chat = new Chat
    {
        UserId = resume.UserId,
        CompanyId = companyId,
        ResumeId = resume.Id,
        VacancyId = vacancy.Id
    };

    await _context.Chats.AddAsync(chat);
    await _context.SaveChangesAsync();

    return chat;
}
    
}