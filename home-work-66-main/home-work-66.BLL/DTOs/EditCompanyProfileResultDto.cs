namespace home_work_66.BLL.DTOs;

public class EditCompanyProfileResultDto
{
    public bool Succeeded { get; set; }
    
    public string? Message { get; set; }
    
    public string? AvatarUrl { get; set; }
    
    public string? CompanyName { get; set; }
    
    public string? Email { get; set; }
}