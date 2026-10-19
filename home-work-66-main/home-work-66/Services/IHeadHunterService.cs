using home_work_66.Models;

namespace home_work_66.Services;

public interface IHeadHunterService
{
    Task<Vacancy?> GetVacancyByIdAsync(int id);
}