namespace home_work_66.BLL.DTOs;

public class CreateVacancyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Salary { get; set; }
    public string Description { get; set; } = "";
    public string Requirements { get; set; } = "";
    public int ExperienceFrom { get; set; }
    public int ExperienceTo { get; set; }
    public int CategoryId { get; set; }
}