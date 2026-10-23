using home_work_66.BLL.Models;

namespace home_work_66.BLL.DTOs;

public class ChatPageDto
{
    public Chat Chat { get; set; } = null!;
    
    public List<Message> Messages { get; set; } = new();
}