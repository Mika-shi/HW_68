using System.ComponentModel.DataAnnotations;

namespace home_work_66.BLL.Models;

public class Vacancy
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Введите название вакансии")]
    public string Title { get; set; } = "";
    [Required(ErrorMessage = "Введите описание вакансии")]
    public string Description { get; set; } = "";
    public string? Requirements { get; set; }
    public decimal Salary { get; set; }
    public int ExperienceFrom { get; set; }
    public int ExperienceTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; } = true;
    public int CompanyId { get; set; }
    public User Company { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
