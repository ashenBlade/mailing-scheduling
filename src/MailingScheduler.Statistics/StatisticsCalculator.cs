using MailingScheduler.Core;

namespace MailingScheduler.Statistics;

public class StatisticsCalculator
{
    private readonly Dictionary<string, Template> _codeTemplate;
    public StatisticsCalculator(Template[] templates)
    {
        _codeTemplate = templates.ToDictionary(x => x.TemplateCode);
    }
    
    public ScheduleStatistics CalculateStatistics(List<Message> scheduledMessages, TimeSpan totalWorkTime, TimeSpan scheduleTime)
    {
        // 1. Кол-во разных шаблонов
        // 2. Статистика для каждого шаблона по отдельности
        // 3. Кол-во различных приоритетов

        var templateGroups = scheduledMessages
                            .GroupBy(m => m.TemplateCode)
                            .Select(g => new TemplateGroup(_codeTemplate[g.Key], g.ToArray()))
                            .ToArray();
        
        return new(
            scheduledMessages.Count, 
            CalculatePerPriorityMessagesCount(templateGroups),
            templateGroups, totalWorkTime, scheduleTime);
    }

    private static Dictionary<Priority, int> CalculatePerPriorityMessagesCount(TemplateGroup[] groups)
    {
        var (rt, h, n, l) = (0, 0, 0, 0);
        foreach (var group in groups)
        {
            switch (group.Template.Priority)
            {
                case Priority.Realtime:
                    rt++;
                    break;
                case Priority.Normal:
                    n++;
                    break;
                case Priority.High:
                    h++;
                    break;
                case Priority.Low:
                    l++;
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