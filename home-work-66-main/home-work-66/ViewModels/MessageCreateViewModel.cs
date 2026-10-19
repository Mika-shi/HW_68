using System.ComponentModel.DataAnnotations;

namespace home_work_66.ViewModels;

public class MessageCreateViewModel
{
    [Required(ErrorMessage = "Введите сообщение")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Длина сообщения не должна превышать 150 символов")]
    public string? Content { get; set; }
    public int ChatId { get; set; }
}