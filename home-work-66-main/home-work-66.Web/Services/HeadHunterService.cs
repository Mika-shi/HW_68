using home_work_66.BLL.DTOs;
using home_work_66.BLL.Interfaces;
using home_work_66.BLL.Models;
using home_work_66.BLL.Enums;
using home_work_66.Services;
using home_work_66.ViewModels;
/*using home_work_66.Enums;* /
/*using home_work_66.ViewModels;*/
using Microsoft.AspNetCore.Identity;

namespace home_work_66.Services;

public class HeadHunterService : IHeadHunterService
{
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly SignInManager<User> _signInManager;
    private readonly IHeadHunterRepository _repository;

    public HeadHunterService(UserManager<User> userManager, IWebHostEnvironment webHostEnvironment, SignInManager<User> signInManager, IHeadHunterRepository repository)
    {
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
        _signInManager = signInManager;
        _repository = repository;
    }

    public async Task<Vacancy?> GetVacancyByIdAsync(int id)
    {
        return await _repository.GetVacancyByIdAsync(id);
    }
    
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _repository.GetCategoriesAsync();
    }
    
    public async Task<VacancyListDto> GetVacanciesAsync(string? searchString, int? categoryId, VacancySortState sortOrder, int page)
    {
        List<Vacancy> vacancies = await _repository.GetPublishedVacanciesAsync();
        
        IEnumerable<Vacancy> query = vacancies;

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
        int count = query.Count();
        int totalPages = (int)Math.Ceiling(count / (double)pageSize);

        List<VacancyDto> items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VacancyDto
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
            })
            .ToList();

        return new VacancyListDto
        {
            Vacancies = items,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = count
        };
    }
    
    public async Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId, bool isOwner)
    {
        List<Vacancy> vacancies = await _repository.GetCompanyVacanciesAsync(companyId);

        if (!isOwner)
        {
            vacancies = vacancies.Where(v => v.IsPublished).ToList();
        }

        return vacancies;
    }
    
    public async Task<bool> CreateVacancyAsync(CreateVacancyDto  model, int companyId)
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

        await _repository.AddVacancyAsync(vacancy);

        return true;
    }
    
    public async Task<CreateVacancyDto?> GetVacancyForEditAsync(int id, int companyId)
    {
        Vacancy? vacancy = await _repository.GetVacancyByIdAndCompanyAsync(id, companyId);

        if (vacancy == null)
        {
            return null;
        }

        return new CreateVacancyDto
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
    
    public async Task<bool> EditVacancyAsync(int id, int companyId, CreateVacancyDto  model)
    {
        Vacancy? vacancy = await _repository.GetVacancyByIdAndCompanyAsync(id, companyId);

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

        await _repository.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> UpdateVacancyAsync(int id, int companyId)
    {
        Vacancy? vacancy = await _repository.GetVacancyByIdAndCompanyAsync(id, companyId);

        if (vacancy == null)
        {
            return false;
        }

        vacancy.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> TogglePublishAsync(int id, int companyId)
    {
        Vacancy? vacancy = await _repository.GetVacancyByIdAndCompanyAsync(id, companyId);

        if (vacancy == null)
        {
            return false;
        }

        vacancy.IsPublished = !vacancy.IsPublished;
        vacancy.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return true;
    }
    
    public async Task<List<CompanyResponsesDto>> GetCompanyResponsesAsync(int companyId)
    {
        List<Chat> chats = await _repository.GetCompanyChatsAsync(companyId);

        return chats
            .GroupBy(c => c.VacancyId)
            .Select(group => new CompanyResponsesDto
            {
                Vacancy = group.First().Vacancy!,
                Responses = group.ToList()
            })
            .ToList();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _repository.GetUserByIdAsync(id);
    }

    public async Task<EditCompanyProfileViewModel?> GetCompanyProfileForEditAsync(int userId)
    {
        User? user = await _repository.GetUserByIdAsync(userId);

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
        User? user = await  _repository.GetUserByIdAsync(userId);

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
    List<Resume> resumes = await _repository.GetResumesByUserAsync(userId);

    if (!isOwner)
    {
        resumes = resumes.Where(r => r.IsPublished).ToList();
    }

    return resumes;
}

    public async Task<int> CreateResumeAsync(Resume resume, int userId)
{
    resume.UserId = userId;
    resume.UpdatedAt = DateTime.UtcNow;
    resume.IsPublished = false;
    
    await _repository.AddResumeAsync(resume);

    return resume.Id;
}

    public async Task<Resume?> GetResumeForEditAsync(int id, int userId)
{
    return await _repository.GetResumeByIdAndUserAsync(id, userId);
}

    public async Task<bool> EditResumeAsync(Resume model, int userId)
{
    Resume? resume = await _repository.GetResumeByIdAndUserAsync(model.Id, userId);

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

    await _repository.SaveChangesAsync();

    return true;
}

    public async Task<bool> UpdateResumeDateAsync(int id, int userId)
{
    Resume? resume = await _repository.GetResumeByIdAndUserAsync(id, userId);

    if (resume == null)
    {
        return false;
    }

    resume.UpdatedAt = DateTime.UtcNow;

    await  _repository.SaveChangesAsync();
    
    return true;
}

    public async Task AddWorkExperienceAsync(WorkExperience workExperience)
{
    await _repository.AddWorkExperienceAsync(workExperience);
}

    public async Task<WorkExperience?> GetWorkExperienceAsync(int id, int userId)
{
    return await _repository.GetWorkExperienceAsync(id, userId);
}

    public async Task UpdateWorkExperienceAsync(WorkExperience workExperience)
{
    await _repository.UpdateWorkExperienceAsync(workExperience);
}

    public async Task<int?> DeleteWorkExperienceAsync(int id, int userId)
{
    WorkExperience? workExperience = await _repository.GetWorkExperienceAsync(id, userId);

    if (workExperience == null)
    {
        return null;
    }

    int resumeId = workExperience.ResumeId;
    
    await _repository.DeleteWorkExperienceAsync(workExperience);

    return resumeId;
}

    public async Task AddEducationAsync(Education education) {
        await _repository.AddEducationAsync(education);
    }

    public async Task<Education?> GetEducationAsync(int id, int userId) {
        return await _repository.GetEducationAsync(id, userId);
    }

    public async Task UpdateEducationAsync(Education education)
    {
        await _repository.UpdateEducationAsync(education);
    }
    
    public async Task<int?> DeleteEducationAsync(int id, int userId)
    {
        Education? education = await _repository.GetEducationAsync(id, userId);

        if (education == null)
        {
            return null;
        }

        int resumeId = education.ResumeId;

        await _repository.DeleteEducationAsync(education);

        return resumeId;
    }

    public async Task AddCourseAsync(Course course)
    {
        await _repository.AddCourseAsync(course);
    }

    public async Task<Course?> GetCourseAsync(int id, int userId)
    {
        return await _repository.GetCourseAsync(id, userId);
    }

    public async Task UpdateCourseAsync(Course course)
    {
        await _repository.UpdateCourseAsync(course);
    }

    public async Task<int?> DeleteCourseAsync(int id, int userId)
    {
        Course? course = await _repository.GetCourseAsync(id, userId);

        if (course == null)
        {
            return null;
        }

        int resumeId = course.ResumeId;

        await _repository.DeleteCourseAsync(course);

        return resumeId;
    }
    public async Task<Resume?> GetResumeDetailsAsync(int id, int userId)
    {
        return await _repository.GetResumeDetailsAsync(id, userId);
    }

    public async Task<bool> ToggleResumePublicationAsync(int id, int userId)
    {
        Resume? resume = await _repository.GetResumeByIdAndUserAsync(id, userId);

        if (resume == null)
        {
            return false;
        }

        resume.IsPublished = !resume.IsPublished;

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<List<Resume>> GetPublishedResumesAsync()
    {
        return await _repository.GetPublishedResumesAsync();
    }

    public async Task<Resume?> GetPublicResumeDetailsAsync(int id)
    {
        return await _repository.GetPublicResumeDetailsAsync(id);
    }
    
    public async Task<List<Category>> GetCategoriesOrderedAsync()
    {
        return await _repository.GetCategoriesOrderedAsync();
    }

    public async Task<List<Resume>> GetPublishedResumesByCategoryAsync(int? categoryId)
    {
        return await _repository.GetPublishedResumesByCategoryAsync(categoryId);
    }
    
    public async Task<Chat> CreateChatAsync(ChatCreateDto model, int userId)
{
    Chat? existingChat = await _repository.GetExistingChatAsync(userId, model.VacancyId);

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

    await _repository.AddChatAsync(chat);

    return chat;
}

    public async Task<ChatPageDto?> GetChatPageAsync(int chatId)
{
    Chat? chat = await _repository.GetChatByIdAsync(chatId);

    if (chat == null)
    {
        return null;
    }

    List<Message> messages = await _repository.GetMessagesByChatIdAsync(chatId);

    return new ChatPageDto
    {
        Chat = chat,
        Messages = messages
    };
}

    public async Task<List<Message>> GetMessagesAsync(int? chatId)
{
    return await _repository.GetMessagesByChatIdAsync(chatId);
}

    public async Task<List<Message>> SendMessageAsync(MessageCreateDto model, int userId)
{
    Message message = new Message
    {
        Content = model.Content,
        UserId = userId,
        CreatedAt = DateTime.UtcNow,
        ChatId = model.ChatId
    };
    
    await _repository.AddMessageAsync(message);

    return await _repository.GetMessagesByChatIdAsync(model.ChatId);
}

    public async Task<Chat?> CreateChatFromResumeAsync(int resumeId, int vacancyId, int companyId)
{
    Resume? resume = await _repository.GetPublishedResumeByIdAsync(resumeId);

    if (resume == null)
    {
        return null;
    }

    Vacancy? vacancy = await _repository.GetVacancyByIdAndCompanyAsync(vacancyId, companyId);

    if (vacancy == null)
    {
        return null;
    }

    Chat? existingChat = await _repository.GetExistingChatForCompanyAsync(resume.UserId, companyId, vacancyId);

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
    
    await _repository.AddChatAsync(chat);

    return chat;
}
    
    public async Task<EditProfileViewModel?> GetUserProfileForEditAsync(int userId)
{
    User? user = await _repository.GetUserByIdAsync(userId);

    if (user == null)
    {
        return null;
    }

    return new EditProfileViewModel
    {
        UserName = user.UserName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber
    };
}

    public async Task<EditUserProfileResult> UpdateUserProfileAsync(int userId, EditProfileViewModel model)
{
    User? user = await _repository.GetUserByIdAsync(userId);

    if (user == null)
    {
        return new EditUserProfileResult
        {
            Succeeded = false,
            Message = "Пользователь не найден"
        };
    }

    user.UserName = model.UserName;
    user.Email = model.Email;
    user.PhoneNumber = model.PhoneNumber;

    if (model.Avatar != null && model.Avatar.Length > 0)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");

        Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Avatar.FileName)}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using FileStream fileStream = new FileStream(filePath, FileMode.Create);
        await model.Avatar.CopyToAsync(fileStream);

        user.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
    }

    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
        return new EditUserProfileResult
        {
            Succeeded = false,
            Message = string.Join("\n", result.Errors.Select(e => e.Description))
        };
    }

    return new EditUserProfileResult
    {
        Succeeded = true,
        AvatarUrl = user.AvatarUrl,
        UserName = user.UserName,
        Email = user.Email
    };
}

    public async Task<List<Resume>> GetUserResumesAsync(int userId)
{
    return await _repository.GetUserResumesAsync(userId);
}

    public async Task<List<Chat>> GetUserResponsesAsync(int userId)
{
    return await _repository.GetUserResponsesAsync(userId);
}
    
    public async Task<AccountResult> RegisterAsync(RegisterViewModel model)
{
    AccountResult accountResult = new AccountResult();

    User? existingUser = await _userManager.FindByEmailAsync(model.Email);

    if (existingUser != null)
    {
        accountResult.Errors.Add("В базе уже существует аккаунт с данным адресом почты");
    }

    bool hasUpper = model.Password.Any(char.IsUpper);
    bool hasLower = model.Password.Any(char.IsLower);
    bool hasDigit = model.Password.Any(char.IsDigit);

    if (!hasUpper || !hasLower || !hasDigit)
    {
        accountResult.Errors.Add("Пароль должен содержать в себе 1 букву верхнего регистра, 1 букву нижнего регистра, а также минимум 1 цифру");
    }

    if (accountResult.Errors.Count > 0)
    {
        accountResult.Succeeded = false;
        return accountResult;
    }

    string avatarPath = "/default.jpg";

    if (model.Avatar != null)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Avatar.FileName)}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using FileStream fileStream = new FileStream(filePath, FileMode.Create);
        await model.Avatar.CopyToAsync(fileStream);

        avatarPath = $"/uploads/avatars/{uniqueFileName}";
    }

    User userToCreate;
    string roleToAssign;

    if (model.IsCompany)
    {
        userToCreate = new User
        {
            Email = model.Email,
            UserName = model.Email,
            CompanyName = model.UserName,
            PhoneNumber = model.PhoneNumber,
            AvatarUrl = avatarPath
        };

        roleToAssign = "company";
    }
    else
    {
        userToCreate = new User
        {
            Email = model.Email,
            UserName = model.UserName,
            PhoneNumber = model.PhoneNumber,
            AvatarUrl = avatarPath
        };

        roleToAssign = "user";
    }

    IdentityResult result = await _userManager.CreateAsync(userToCreate, model.Password);

    if (!result.Succeeded)
    {
        accountResult.Succeeded = false;

        foreach (IdentityError error in result.Errors)
        {
            accountResult.Errors.Add(error.Description);
        }

        return accountResult;
    }

    await _userManager.AddToRoleAsync(userToCreate, roleToAssign);
    await _signInManager.SignInAsync(userToCreate, isPersistent: false);

    accountResult.Succeeded = true;
    accountResult.IsCompany = model.IsCompany;

    return accountResult;
}
    
    public async Task<AccountResult> LoginAsync(LoginViewModel model)
    {
        User? user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            return new AccountResult
            {
                Succeeded = false,
                Message = "Неправильный логин и (или) пароль"
            };
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            false
        );

        if (!result.Succeeded)
        {
            return new AccountResult
            {
                Succeeded = false,
                Message = "Неправильный логин и (или) пароль"
            };
        }

        bool isCompany = await _userManager.IsInRoleAsync(user, "company");

        return new AccountResult
        {
            Succeeded = true,
            IsCompany = isCompany
        };
    }
    
    public async Task LogOffAsync()
    {
        await _signInManager.SignOutAsync();
    }
    
    public async Task<bool> IsEmailAvailableAsync(string email, int? id)
    {
        return await _repository.IsEmailAvailableAsync(email, id);
    }

    public bool IsPasswordValid(string password)
    {
        return password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit);
    }
    
    public async Task<Category?> CreateCategoryAsync(string name)
    {
        string trimmedName = name.Trim();

        bool exists = await _repository.CategoryExistsAsync(trimmedName);

        if (exists)
        {
            return null;
        }

        Category category = new Category
        {
            Name = trimmedName
        };

        await _repository.AddCategoryAsync(category);

        return category;
    }

    public async Task<string?> DeleteCategoryAsync(int id)
    {
        Category? category = await _repository.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return "Категория не найдена";
        }

        bool hasResumes = await _repository.CategoryHasResumesAsync(id);
        bool hasVacancies = await _repository.CategoryHasVacanciesAsync(id);

        if (hasResumes || hasVacancies)
        {
            return "Нельзя удалить категорию, так как к ней привязаны резюме или вакансии";
        }
        
        await _repository.DeleteCategoryAsync(category);

        return null;
    }
    
}