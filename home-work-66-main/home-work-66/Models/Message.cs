namespace home_work_66.Models;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int ChatId { get; set; }
}