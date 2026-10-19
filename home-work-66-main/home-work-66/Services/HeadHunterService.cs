using home_work_66.Enums;
using home_work_66.Models;
using home_work_66.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace home_work_66.Services;

public class HeadHunterService : IHeadHunterService
{
    private readonly HeadHunterContext _context;

    public HeadHunterService(HeadHunterContext context)
    {
        _context = context;
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
}