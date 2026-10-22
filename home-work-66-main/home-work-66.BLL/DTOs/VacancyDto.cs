using home_work_66.BLL.Models;

namespace home_work_66.BLL.DTOs;

public class VacancyDto
{
    public int Id { get; set; }
    
    public string Title { get; set; } = "";
    
    public decimal Salary { get; set; }
    
    public string Description { get; set; } = "";
    
    public string Requirements { get; set; } = "";
    
    public int ExperienceFrom { get; set; }
    
    public int ExperienceTo { get; set; }
    
    public int CategoryId { get; set; }
    
    public int CompanyId { get; set; }
    
    public bool IsPublished { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    public Category? Category { get; set; }
    
    public User? Company { get; set; }
}