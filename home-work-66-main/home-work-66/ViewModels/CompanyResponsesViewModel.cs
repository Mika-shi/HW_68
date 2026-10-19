using home_work_66.Models;

namespace home_work_66.ViewModels;

public class CompanyResponsesViewModel
{
    public Vacancy Vacancy { get; set; } = null!;
    public List<Chat> Responses { get; set; } = new();
}