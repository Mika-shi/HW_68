using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace home_work_66.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя")]
    [Display(Name = "Имя пользователя")]
    public string? UserName { get; set; }
    [Required(ErrorMessage = "Введите Email")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    [Display(Name = "Адрес электронной почты")]
    [Remote("CheckEmail", "Validation", ErrorMessage = "В базе уже существует аккаунт с данным адресом почты")]
    public string? Email { get; set; }
    [Display(Name = "Аватар")]
    public IFormFile? Avatar { get; set; }
    [Required(ErrorMessage = "Введите номер телефона")]
    [Phone(ErrorMessage = "Некорректный формат номера телефона")]
    [Display(Name = "Номер телефона")]
    public string? PhoneNumber { get; set; }
    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Длина должна быть не менее 6 символов")]
    [Remote("CheckPassword", "Validation", ErrorMessage = "Пароль должен содержать в себе 1 букву верхнего регистра, 1 букву нижнего регистра, а также минимум 1 цифру")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
    public bool IsCompany { get; set; }
}