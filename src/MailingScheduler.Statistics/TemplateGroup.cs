using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy;
using MailingScheduler.PlanningStrategy.PlanningStrategy;
using MailingScheduler.PlanningStrategy.PriorityMessageChecker;

namespace MailingScheduler.Statistics;

public class TemplateGroup
{
    public TemplateGroup(Template template, Message[] messages)
    {
        Template = template;
        Messages = messages;
    }
    public Template Template { get; set; }
    public Message[] Messages { get; set; }

    public TemplateGroupSummary CalculateSummary()
    {
        int? priorityCount = null;
        TemplateDistribution distribution = TemplateDistribution.Uniform;
        if (Template.Strategy is PrioritizedPlanningStrategy prioritized)
        {
            priorityCount = Messages.Count(m => prioritized.PriorityChecker.IsPrioritized(m));
            distribution = prioritized.Distribution;
        }

        return new TemplateGroupSummary(Template.TemplateCode, Template.Priority, Messages.Length, priorityCount,
            distribution);
    }
}