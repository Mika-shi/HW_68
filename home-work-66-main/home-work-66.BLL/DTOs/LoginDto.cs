namespace home_work_66.BLL.DTOs;

public class LoginDto
{
    public string Email { get; set; } = "";
    
    public string Password { get; set; } = "";
    
    public bool RememberMe { get; set; }
}