using System.ComponentModel.DataAnnotations;

namespace home_work_66.BLL.Models;

public class Category
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Введите название категории")]
    public string Name { get; set; } = "";
}