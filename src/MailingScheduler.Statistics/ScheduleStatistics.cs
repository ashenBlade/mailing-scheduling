using MailingScheduler.Core;

namespace MailingScheduler.Statistics;

public class ScheduleStatistics
{
    public ScheduleStatistics(int totalMessagesCount, Dictionary<Priority, int> priorityMessagesCount, TemplateGroup[] templateGroups)
    {
        TotalMessagesCount = totalMessagesCount;
        PriorityMessagesCount = priorityMessagesCount;
        TemplateGroups = templateGroups;
    }
    
    /// <summary>
    /// Общее число сообщений
    /// </summary>
    public int TotalMessagesCount { get; }

    /// <summary>
    /// Количество различных шаблонов
    /// </summary>
    public int DifferentTemplatesCount => TemplateGroups.Length;
    
    /// <summary>
    /// Количество сообщений с данным приоритетом
    /// </summary>
    public Dictionary<Priority, int> PriorityMessagesCount { get; }

    public TemplateGroup[] TemplateGroups { get; }
}