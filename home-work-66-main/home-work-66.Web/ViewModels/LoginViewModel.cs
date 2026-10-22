using System.ComponentModel.DataAnnotations;

namespace home_work_66.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите email")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }
    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}