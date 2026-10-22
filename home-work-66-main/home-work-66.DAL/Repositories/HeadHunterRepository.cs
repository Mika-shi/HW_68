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
}