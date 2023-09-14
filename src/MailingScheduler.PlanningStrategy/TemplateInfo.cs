using MailingScheduler.Core;

namespace MailingScheduler.PlanningStrategy;

public record TemplateInfo(string TemplateCode, Priority Priority, TemplateDistribution Distribution, int MaxSendSpeed);