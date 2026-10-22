using System.ComponentModel.DataAnnotations;

namespace home_work_66.BLL.Models;

public class Education
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название учебного заведения")]
    public string InstitutionName { get; set; } = "";

    [Required(ErrorMessage = "Введите специальность")]
    public string Specialty { get; set; } = "";

    [Required(ErrorMessage = "Введите год окончания")]
    public int GraduationYear { get; set; }

    public int ResumeId { get; set; }

    public Resume? Resume { get; set; }
}