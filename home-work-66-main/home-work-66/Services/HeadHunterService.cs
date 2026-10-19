using home_work_66.Models;
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
}