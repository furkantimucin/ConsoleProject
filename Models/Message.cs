namespace ConsoleProject.Models;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int SenderId { get; set; }
    public Users Sender { get; set; }
    public DateTime CreatedAt { get; set; }
}