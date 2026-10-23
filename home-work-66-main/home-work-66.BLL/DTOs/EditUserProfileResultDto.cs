namespace home_work_66.BLL.DTOs;

public class EditUserProfileResultDto
{
    public bool Succeeded { get; set; }
    
    public string? Message { get; set; }
    
    public string? AvatarUrl { get; set; }
    
    public string? UserName { get; set; }
    
    public string? Email { get; set; }
}