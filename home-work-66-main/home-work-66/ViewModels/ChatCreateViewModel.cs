using System.ComponentModel.DataAnnotations;
using home_work_66.Models;

namespace home_work_66.ViewModels;

public class ChatCreateViewModel
{
    public int CompanyId { get; set; }
    
    public int VacancyId { get; set; }

    [Required(ErrorMessage = "Пожалуйста, выберите резюме")]
    public int? ResumeId { get; set; }

    public List<Resume> Resumes { get; set; } = new();
}