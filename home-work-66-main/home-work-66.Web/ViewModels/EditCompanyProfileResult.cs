namespace home_work_66.ViewModels;

public class EditCompanyProfileResult
{
    public bool Succeeded  { get; set; }
    public string? Message { get; set; }
    public string? AvatarUrl { get; set; }
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
}