namespace MailingScheduler.Database;

public class Message
{
    public Guid Id { get; set; }
    public string TemplateCode { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ClientTimezoneOffset { get; set; }
}