namespace home_work_66.BLL.DTOs;

public class EditCompanyProfileDto
{
    public string CompanyName { get; set; } = "";
    
    public string PhoneNumber { get; set; } = "";
    
    public string Email { get; set; } = "";
    
    public string? AvatarUrl { get; set; }
}