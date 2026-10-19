using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace home_work_66.ViewModels;

public class EditCompanyProfileViewModel
{
    [Required(ErrorMessage = "Введите название компании")]
    [Display(Name = "Название компании")]
    public string CompanyName { get; set; } = "";

    [Required(ErrorMessage = "Введите номер телефона")]
    [Display(Name = "Номер телефона")]
    public string PhoneNumber { get; set; } = "";

    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Введите корректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    public string? AvatarUrl { get; set; }

    [Display(Name = "Новый логотип / аватар")]
    public IFormFile? Avatar { get; set; }
}