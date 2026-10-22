using home_work_66.BLL.Interfaces;
using home_work_66.BLL.Models;
using home_work_66.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.DAL.Repositories;

public class HeadHunterRepository : IHeadHunterRepository
{
    private readonly HeadHunterContext _context;

    public HeadHunterRepository(HeadHunterContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesOrderedAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<bool> CategoryExistsAsync(string name)
    {
        return await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> CategoryHasResumesAsync(int id)
    {
        return await _context.Resumes.AnyAsync(r => r.CategoryId == id);
    }

    public async Task<bool> CategoryHasVacanciesAsync(int id)
    {
        return await _context.Vacancies.AnyAsync(v => v.CategoryId == id);
    }

    public async Task AddCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public async Task<Vacancy?> GetVacancyByIdAsync(int id)
    {
        return await _context.Vacancies
            .Include(v => v.Company)
            .Include(v => v.Category)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Vacancy>> GetVacanciesAsync()
    {
        return await _context.Vacancies
            .Include(v => v.Company)
            .Include(v => v.Category)
            .ToListAsync();
    }

    public async Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId)
    {
        return await _context.Vacancies
            .Where(v => v.CompanyId == companyId)
            .Include(v => v.Category)
            .ToListAsync();
    }

    public async Task AddVacancyAsync(Vacancy vacancy)
    {
        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync();
    }

    public async Task<Vacancy?> GetVacancyForEditAsync(int id, int companyId)
    {
        return await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);
    }

    public async Task<List<Vacancy>> GetPublishedVacanciesAsync()
    {
        return await _context.Vacancies
            .Include(v => v.Category)
            .Include(v => v.Company)
            .Where(v => v.IsPublished)
            .ToListAsync();
    }

    public async Task<Vacancy?> GetVacancyByIdAndCompanyAsync(int id, int companyId)
    {
        return await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == id && v.CompanyId == companyId);
    }

    public async Task<List<Chat>> GetCompanyChatsAsync(int companyId)
    {
        return await _context.Chats
            .Where(c => c.CompanyId == companyId)
            .Include(c => c.Vacancy)
            .Include(c => c.User)
            .Include(c => c.Resume)
            .OrderByDescending(c => c.Id)
            .ToListAsync();
    }
    
    public async Task<List<Resume>> GetResumesByUserAsync(int userId)
{
    return await _context.Resumes
        .Include(r => r.Category)
        .Where(r => r.UserId == userId)
        .OrderByDescending(r => r.UpdatedAt)
        .ToListAsync();
}

public async Task<Resume?> GetResumeByIdAndUserAsync(int id, int userId)
{
    return await _context.Resumes
        .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
}

public async Task AddResumeAsync(Resume resume)
{
    _context.Resumes.Add(resume);
    await _context.SaveChangesAsync();
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

public async Task DeleteWorkExperienceAsync(WorkExperience workExperience)
{
    _context.WorkExperiences.Remove(workExperience);
    await _context.SaveChangesAsync();
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

public async Task DeleteEducationAsync(Education education)
{
    _context.Educations.Remove(education);
    await _context.SaveChangesAsync();
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

public async Task DeleteCourseAsync(Course course)
{
    _context.Courses.Remove(course);
    await _context.SaveChangesAsync();
}

public async Task<Chat?> GetExistingChatAsync(int userId, int vacancyId)
{
    return await _context.Chats
        .FirstOrDefaultAsync(c => c.UserId == userId && c.VacancyId == vacancyId);
}

public async Task AddChatAsync(Chat chat)
{
    await _context.Chats.AddAsync(chat);
    await _context.SaveChangesAsync();
}

public async Task<Chat?> GetChatByIdAsync(int chatId)
{
    return await _context.Chats
        .Include(x => x.Vacancy)
        .Include(x => x.Resume)
        .Include(x => x.User)
        .Include(x => x.Company)
        .FirstOrDefaultAsync(x => x.Id == chatId);
}

public async Task<List<Message>> GetMessagesByChatIdAsync(int? chatId)
{
    return await _context.Messages
        .Where(x => x.ChatId == chatId)
        .Include(x => x.User)
        .OrderBy(x => x.CreatedAt)
        .ToListAsync();
}

public async Task AddMessageAsync(Message message)
{
    await _context.Messages.AddAsync(message);
    await _context.SaveChangesAsync();
}

public async Task<Resume?> GetPublishedResumeByIdAsync(int resumeId)
{
    return await _context.Resumes
        .FirstOrDefaultAsync(r => r.Id == resumeId && r.IsPublished);
}

public async Task<Chat?> GetExistingChatForCompanyAsync(int userId, int companyId, int vacancyId)
{
    return await _context.Chats
        .FirstOrDefaultAsync(c =>
            c.UserId == userId &&
            c.CompanyId == companyId &&
            c.VacancyId == vacancyId);
}

}