using System.ComponentModel.DataAnnotations;

namespace home_work_66.Models;

public class Resume
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название резюме")]
    public string Title { get; set; } = "";
    
    [Required(ErrorMessage = "Выберите категорию")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required(ErrorMessage = "Укажите ожидаемую зарплату")]
    public decimal? Salary { get; set; }

    [Required(ErrorMessage = "Введите Telegram")]
    public string? Telegram { get; set; } = "";

    [Required(ErrorMessage = "Введите Email")]
    [EmailAddress(ErrorMessage = "Введите корректный Email")]
    public string? Email  { get; set; } = "";

    [Required(ErrorMessage = "Введите номер телефона")]
    public string? PhoneNumber { get; set; } = "";

    public string?  Facebook { get; set; } 

    public string? LinkedIn { get; set; } 

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPublished { get; set; }

    public int  UserId { get; set; } 

    public User? User { get; set; }
    
    public List<WorkExperience> WorkExperiences { get; set; } = new();
    public List<Education> Educations { get; set; } = new();
    public List<Course> Courses { get; set; } = new();
}