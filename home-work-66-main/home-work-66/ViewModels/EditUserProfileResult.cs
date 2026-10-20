namespace home_work_66.ViewModels;

public class EditUserProfileResult
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public string? AvatarUrl { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
}