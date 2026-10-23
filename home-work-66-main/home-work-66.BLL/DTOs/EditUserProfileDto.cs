namespace home_work_66.BLL.DTOs;

public class EditUserProfileDto
{
    public string UserName { get; set; } = "";
    
    public string Email { get; set; } = "";
    
    public string PhoneNumber { get; set; } = "";
    
    public string? AvatarUrl { get; set; }
}