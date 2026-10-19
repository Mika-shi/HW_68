using home_work_66.Enums;
using home_work_66.Models;
using home_work_66.ViewModels;

namespace home_work_66.Services;

public interface IHeadHunterService
{
    Task<Vacancy?> GetVacancyByIdAsync(int id);
    Task<VacancyIndexViewModel> GetVacanciesAsync(string? searchString, int? categoryId, VacancySortState sortOrder, int page);
    Task<List<Category>> GetCategoriesAsync();
    
    Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId, bool isOwner);
}