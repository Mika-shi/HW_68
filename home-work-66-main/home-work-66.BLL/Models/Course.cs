

namespace home_work_66.BLL.Models;

public class Course
{
    public int Id { get; set; }

    public string CourseName { get; set; } = "";
    public string OrganizationName { get; set; } = "";
    public int Year { get; set; }

    public int ResumeId { get; set; }
    public Resume? Resume { get; set; }
}