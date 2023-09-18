using MailingScheduler.Core;

namespace MailingScheduler.Statistics;

public class StatisticsCalculator
{
    private readonly Message[] _sourceMessages;
    private readonly Dictionary<string, Template> _codeTemplate;
    public StatisticsCalculator(Template[] templates, Message[] sourceMessages)
    {
        _sourceMessages = sourceMessages;
        _codeTemplate = templates.ToDictionary(x => x.TemplateCode);
    }
    
    public ScheduleStatistics CalculateStatistics(List<Message> scheduledMessages, TimeSpan totalWorkTime, TimeSpan scheduleTime)
    {
        var scheduledTemplateGroups = CalculateTemplateGroups(scheduledMessages);
        var totalTemplateGroups = CalculateTemplateGroups(_sourceMessages);
        var countByPriority = CalculatePerPriorityMessagesCount(scheduledTemplateGroups);
        
        return new ScheduleStatistics(
            _sourceMessages.Length,
            scheduledMessages.Count, 
            countByPriority,
            scheduledTemplateGroups, totalTemplateGroups,  
            totalWorkTime, scheduleTime);
    }
    
    private TemplateGroup[] CalculateTemplateGroups(IEnumerable<Message> messages) => 
        messages
           .GroupBy(m => m.TemplateCode)
           .Select(g => new TemplateGroup(_codeTemplate[g.Key], g.ToArray()))
           .ToArray();

    private static Dictionary<Priority, int> CalculatePerPriorityMessagesCount(TemplateGroup[] groups)
    {
        var (rt, h, n, l) = (0, 0, 0, 0);
        foreach (var group in groups)
        {
            var count = group.Messages.Length;
            switch (group.Template.Priority)
            {
                case Priority.Realtime:
                    rt += count;
                    break;
                case Priority.Normal:
                    n += count;
                    break;
                case Priority.High:
                    h += count;
                    break;
                case Priority.Low:
                    l += count;
                    break;
            }
        }
        
        return new()
        {
            {Priority.Realtime, rt},
            {Priority.High, h},
            {Priority.Normal, n},
            {Priority.Low, l},
        };
    }
}