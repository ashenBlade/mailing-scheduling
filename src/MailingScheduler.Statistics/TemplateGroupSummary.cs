using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy;

namespace MailingScheduler.Statistics;

public readonly record struct TemplateGroupSummary(string TemplateCode, Priority Priority, int TotalMessagesCount, int? PriorityMessagesCount, TemplateDistribution Distribution);