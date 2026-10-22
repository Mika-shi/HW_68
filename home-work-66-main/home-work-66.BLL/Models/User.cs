using Microsoft.AspNetCore.Identity;

namespace home_work_66.BLL.Models;

public class User: IdentityUser<int>
{
   public string AvatarUrl { get; set; }
   public string? CompanyName { get; set; }
}