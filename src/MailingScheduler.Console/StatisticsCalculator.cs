using MailingScheduler.Core;

namespace MailingScheduler.Console;

public class StatisticsCalculator
{
    private readonly Dictionary<string, Template> _codeTemplate;
    public StatisticsCalculator(Template[] templates)
    {
        _codeTemplate = templates.ToDictionary(x => x.TemplateCode);
    }
    
    public ScheduleStatistics CalculateStatistics(Message[] scheduledMessages)
    {
        // 1. Кол-во разных шаблонов
        // 2. Статистика для каждого шаблона по отдельности
        // 3. Кол-во различных приоритетов

        var templateGroups = scheduledMessages
                            .GroupBy(m => m.TemplateCode)
                            .Select(g => new TemplateGroup(_codeTemplate[g.Key], g.ToArray()))
                            .ToList();
        var counts = new Dictionary<Priority, int>(4)
                     {
                         {Priority.Realtime, 0},
                         {Priority.Realtime, 0},
                         {Priority.Realtime, 0},
                         {Priority.Realtime, 0},
                     };
        foreach (var group in templateGroups)
        {
            
        }
        return new(scheduledMessages.Length, templateGroups.Count, )
               {
                   TotalMessagesCount = scheduledMessages.Length,
                   DifferentTemplatesCount = templateGroups.Count,
               };
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
    }
}

public class TemplateGroup
{
    public TemplateGroup(Template template, Message[] messages)
    {
        Template = template;
        Messages = messages;
    }
    public Template Template { get; set; }
    public Message[] Messages { get; set; }
}

public class ScheduleStatistics
{
    public ScheduleStatistics(int totalMessagesCount, int differentTemplatesCount, Dictionary<Priority, int> priorityMessagesCount)
    {
        TotalMessagesCount = totalMessagesCount;
        DifferentTemplatesCount = differentTemplatesCount;
        PriorityMessagesCount = priorityMessagesCount;
    }
    
    /// <summary>
    /// Общее число сообщений
    /// </summary>
    public int TotalMessagesCount { get; set; }
    
    /// <summary>
    /// Количество различных шаблонов
    /// </summary>
    public int DifferentTemplatesCount { get; set; }
    
    /// <summary>
    /// Количество сообщений с данным приоритетом
    /// </summary>
    public Dictionary<Priority, int> PriorityMessagesCount { get; set; }
}