using System.ComponentModel.DataAnnotations.Schema;


namespace home_work_66.BLL.Models;

public class Chat
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public int CompanyId { get; set; }
    public User? Company { get; set; }
    
    public int ResumeId { get; set; }
    public Resume? Resume { get; set; }
    
    public int VacancyId { get; set; }
    public Vacancy? Vacancy { get; set; }
}