using home_work_66.BLL.Models;

namespace home_work_66.BLL.DTOs;

public class CompanyResponsesDto
{
    public Vacancy Vacancy { get; set; } = null!;
    public List<Chat> Responses { get; set; } = new();
}