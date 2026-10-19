using System.ComponentModel.DataAnnotations;

namespace home_work_66.ViewModels;

public class CreateVacancyViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название вакансии")]
    [Display(Name = "Название вакансии")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Укажите заработную плату")]
    [Range(0, double.MaxValue, ErrorMessage = "Зарплата не может быть отрицательной")]
    [Display(Name = "Заработная плата")]
    public decimal Salary { get; set; }

    [Required(ErrorMessage = "Введите описание вакансии")]
    [Display(Name = "Детальное описание")]
    public string Description { get; set; } = "";

    [Display(Name = "Требования")]
    public string? Requirements { get; set; }

    [Display(Name = "Опыт работы от (лет)")]
    [Range(0, 100, ErrorMessage = "Опыт работы должен быть от 0 до 100")]
    public int ExperienceFrom { get; set; }

    [Display(Name = "Опыт работы до (лет)")]
    [Range(0, 100, ErrorMessage = "Опыт работы должен быть от 0 до 100")]
    public int ExperienceTo { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }
}