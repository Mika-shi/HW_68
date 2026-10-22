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
}