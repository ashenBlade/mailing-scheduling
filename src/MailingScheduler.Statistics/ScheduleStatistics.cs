using System.Text.RegularExpressions;
using MailingScheduler.Core;

namespace MailingScheduler.Statistics;

public class ScheduleStatistics
{
    public ScheduleStatistics(int totalMessagesCount,
                              int scheduledMessagesCount, 
                              Dictionary<Priority, int> priorityMessagesCount, 
                              TemplateGroup[] scheduledTemplateGroups, 
                              TemplateGroup[] totalTemplateGroups,
                              TimeSpan totalWorkTime,
                              TimeSpan scheduleTime)
    {
        TotalMessagesCount = totalMessagesCount;
        ScheduledMessagesCount = scheduledMessagesCount;
        PriorityMessagesCount = priorityMessagesCount;
        ScheduledTemplateGroups = scheduledTemplateGroups;
        TotalWorkTime = totalWorkTime;
        ScheduleTime = scheduleTime;
        TotalTemplateGroups = totalTemplateGroups;
    }

    /// <summary>
    /// Общее число сообщений
    /// </summary>
    public int TotalMessagesCount { get; }

    /// <summary>
    /// Количество запланированных сообщений
    /// </summary>
    public int ScheduledMessagesCount { get; }
    
    /// <summary>
    /// Количество сообщений с данным приоритетом
    /// </summary>
    public Dictionary<Priority, int> PriorityMessagesCount { get; }

    /// <summary>
    /// Группы запланированных сообщений по шаблонам
    /// </summary>
    public TemplateGroup[] ScheduledTemplateGroups { get; }

    /// <summary>
    /// Группы сообщений всего
    /// </summary>
    public TemplateGroup[] TotalTemplateGroups { get; }
    /// <summary>
    /// Общее время работы: от запуска до завершения
    /// </summary>
    public TimeSpan TotalWorkTime { get; }
    
    /// <summary>
    /// Время затраченное на планирование
    /// </summary>
    public TimeSpan ScheduleTime { get; }

    public IEnumerable<(TemplateGroup Total, TemplateGroup? Scheduled)> GetTotalAndScheduledGroups()
    {
        var dict = ScheduledTemplateGroups.ToDictionary(x => x.Template.TemplateCode);
        foreach (var group in TotalTemplateGroups)
        {
            if (dict.TryGetValue(group.Template.TemplateCode, out var scheduled))
            {
                yield return ( group, scheduled );
            }
            else
            {
                yield return ( group, null );
            }
        }
    }
}