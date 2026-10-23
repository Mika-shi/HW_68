namespace home_work_66.BLL.DTOs;

public class AccountResultDto
{
    public bool Succeeded { get; set; }
    
    public bool IsCompany { get; set; }
    
    public string? Message { get; set; }
    
    public List<string> Errors { get; set; } = new();
}