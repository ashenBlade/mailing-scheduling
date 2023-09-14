using System.ComponentModel.DataAnnotations;

namespace MailingScheduler.Database;

public class Template
{
    [Key]
    [Required]
    public string TemplateCode { get; set; } = null!;
    public DistributionType Distribution { get; set; }
    public TemplatePriority Priority { get; set; }
    public int MaxSendSpeed { get; set; }
}