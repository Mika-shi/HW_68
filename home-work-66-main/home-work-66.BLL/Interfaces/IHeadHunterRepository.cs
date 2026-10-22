using home_work_66.BLL.Models;

namespace home_work_66.BLL.Interfaces;

public interface IHeadHunterRepository
{
    Task<User?> GetUserByIdAsync(int id);

    Task<List<Category>> GetCategoriesAsync();
    Task<List<Category>> GetCategoriesOrderedAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<bool> CategoryExistsAsync(string name);
    Task<bool> CategoryHasResumesAsync(int id);
    Task<bool> CategoryHasVacanciesAsync(int id);
    Task AddCategoryAsync(Category category);
    Task DeleteCategoryAsync(Category category);
    Task SaveChangesAsync();
    Task<Vacancy?> GetVacancyByIdAsync(int id);
    Task<List<Vacancy>> GetVacanciesAsync();
    Task<List<Vacancy>> GetCompanyVacanciesAsync(int companyId);
    Task AddVacancyAsync(Vacancy vacancy);
    Task<Vacancy?> GetVacancyForEditAsync(int id, int companyId);
    Task<List<Vacancy>> GetPublishedVacanciesAsync();
    Task<Vacancy?> GetVacancyByIdAndCompanyAsync(int id, int companyId);
    Task<List<Chat>> GetCompanyChatsAsync(int companyId);
}