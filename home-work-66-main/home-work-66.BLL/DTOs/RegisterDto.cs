namespace home_work_66.BLL.DTOs;

public class RegisterDto
{
    public string Email { get; set; } = "";
    
    public string UserName { get; set; } = "";
    
    public string PhoneNumber { get; set; } = "";
    
    public string Password { get; set; } = "";
    
    public bool IsCompany { get; set; }
    
    public string AvatarUrl { get; set; } = "/default.jpg";
}