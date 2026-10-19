using System.ComponentModel.DataAnnotations;

namespace home_work_66.ViewModels;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Введите имя")]
    [Display(Name = "Имя")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Введите email")]
    [EmailAddress(ErrorMessage = "Введите корректный email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите номер телефона")]
    [Phone(ErrorMessage = "Введите корректный номер телефона")]
    [StringLength(13, MinimumLength = 8, ErrorMessage = "Номер телефона должен содержать от 8 до 13 символов")]
    [Display(Name = "Номер телефона")]
    public string PhoneNumber { get; set; } = "";

    [Display(Name = "Новый аватар")]
    public IFormFile? Avatar { get; set; }
}