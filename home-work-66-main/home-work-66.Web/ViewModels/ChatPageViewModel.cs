using home_work_66.BLL.Models;

namespace home_work_66.ViewModels;

public class ChatPageViewModel
{
    public Chat? Chat { get; set; }
    public List<Message> Messages { get; set; } = new();
    public MessageCreateViewModel CreateForm { get; set; } = new();
}