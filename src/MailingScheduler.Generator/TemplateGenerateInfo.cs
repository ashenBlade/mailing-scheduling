using MailingScheduler.PlanningStrategy;

namespace MailingScheduler.Generator;

public record TemplateGenerateInfo(TemplateInfo TemplateInfo, int TotalMessagesCount, int? PriorityCount);