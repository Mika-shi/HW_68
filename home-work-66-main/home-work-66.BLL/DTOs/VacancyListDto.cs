namespace home_work_66.BLL.DTOs;

public class VacancyListDto
{
    public List<VacancyDto> Vacancies { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}