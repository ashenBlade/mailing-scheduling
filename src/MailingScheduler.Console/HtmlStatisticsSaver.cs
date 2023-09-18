using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy;
using MailingScheduler.Statistics;
using Serilog;

namespace MailingScheduler.Console;

public class HtmlStatisticsSaver
{
    private readonly string _filename;

    public HtmlStatisticsSaver(string filename)
    {
        _filename = filename;
    }
    
    public void SaveStatistics(ScheduleStatistics statistics)
    {
        Log.Debug("Начинаю запись в файл: {FileName}", _filename);
        using var file = File.OpenWrite(_filename);
        using var writer = new StreamWriter(file);
        // Записываем заголовок
        writer.Write(@"<html><head><meta charset=""utf-8""/><title>Статистика по планировщику</title></head><body>");
        try
        {
            WriteStatistics(statistics, writer);
        }
        finally
        {
            writer.Write("</body></html>");
        }
    }

    private void WriteStatistics(ScheduleStatistics statistics, StreamWriter writer)
    {
        // 1. Общее кол-во сообщений
        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Общее количество сообщений: {0}", statistics.TotalMessagesCount);    
        }
        
        writer.Write("<ul><li>Общее: {0}</li><li>Выбрано: {1}</li></ul>", 
            statistics.TotalMessagesCount, statistics.ScheduledMessagesCount);

        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Количество отобранных сообщений: {0}", statistics.ScheduledMessagesCount);
        }

        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Время планирования: {0:g}", statistics.ScheduleTime);
        }

        // 2. Кол-во различных шаблонов
        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Количество различных шаблонов:\n");
        }
        
        writer.Write("<ul><li>Общее: {0}</li><li>Выбрано: {1}</li></ul>", 
            statistics.TotalTemplateGroups.Length, statistics.ScheduledTemplateGroups.Length);
        
        // 3. Кол-во сообщений по приоритетам
        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Количество взятых сообщений по приоритетам");
        }

        writer.Write("<table style=\"border: 1px\">");
        writer.Write("<thead><tr><th>Приоритет</th><th>Число сообщений</th></tr></thead>");
        writer.Write("<tbody>");
        foreach (var priority in Enum.GetValues<Priority>())
        {
            var count = statistics.PriorityMessagesCount[priority];
            writer.Write("<tr><td>{0}</td><td>{1}</td></tr>", priority.ToString(), count);
        }
        writer.Write("</tbody>");
        writer.Write("</table>");
        
        
        // 4. Таблица со всеми сообщениями/шаблонами
        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Общая сводка");
        }

        writer.Write("<table style=\"border: 1px\">");
        writer.Write("<thead><tr>"
                   + "<th>Шаблон</th>"
                   + "<th>Приоритет</th>"
                   + "<th>Распределение</th>"
                   + "<th>Сообщений всего (общее/взято)</th>"
                   + "<th>Приоритетные (общее/взято)</th>"
                   + "<th>Неприоритетных (общее/взято)</th>"
                   + "</tr></thead>");
        writer.Write("<tbody>\n");
        
        foreach (var (group, scheduled) in statistics.GetTotalAndScheduledGroups()
                                                     .OrderBy(x => (int)x.Total.Template.Priority)
                                                     .ThenByDescending(x => x.Scheduled is null 
                                                                                ? int.MinValue 
                                                                                : x.Scheduled.Messages.Length))
        {
            var totalSummary = group.CalculateSummary();

            var (totalMessages, totalPriorityMessages, totalNonPriorityMessages) = ExtractData(totalSummary);
            var (scheduledMessages, scheduledPriority, scheduledNonPriority) =
                scheduled is not null 
                    ? ExtractData(scheduled.CalculateSummary()) 
                    : (0, "", "");
            
            writer.Write("<tr>"
                       + "<td>{0}</td>"
                       + "<td>{1}</td>"
                       + "<td>{2}</td>"
                       + "<td>{3} / {4}</td>"
                       + "<td>{5} / {6}</td>"
                       + "<td>{7} / {8}</td>"
                       + "</tr>\n",
                totalSummary.TemplateCode, 
                ToString(totalSummary.Priority),
                ToString(totalSummary.Distribution),
                totalMessages, scheduledMessages,
                totalPriorityMessages, scheduledPriority,
                totalNonPriorityMessages, scheduledNonPriority);
        }
        writer.Write("</tbody>");
        writer.Write("</table>");
    }

    private static (int MessagesCount, string Priority, string Scheduled) ExtractData(TemplateGroupSummary summary)
    {
        if (summary.PriorityMessagesCount is {} priorityMessagesCount)
        {
            return ( summary.TotalMessagesCount, priorityMessagesCount.ToString(),
                     ( summary.TotalMessagesCount - priorityMessagesCount ).ToString() );
        }

        return ( summary.TotalMessagesCount, "", "" );
    }
    

    private static string ToString(Priority priority) => 
        priority switch
        {
            Priority.Realtime => "RT",
            Priority.High     => "H",
            Priority.Normal   => "N",
            Priority.Low      => "L",
            _ => throw new ArgumentOutOfRangeException(
                     nameof(priority), priority, null)
        };

    private static string ToString(TemplateDistribution distribution) => 
        distribution switch
        {
            TemplateDistribution.Daytime => "Дневное",
            TemplateDistribution.Evening => "Вечернее",
            TemplateDistribution.Morning => "Утреннее",
            TemplateDistribution.Uniform =>
                "Равномерное",
            _ => throw new ArgumentOutOfRangeException(nameof(distribution), distribution, null)
        };

}

internal static class StreamWriterExtensions
{
    public static ParagraphWriterDisposable BeginParagraph(this StreamWriter writer)
    {
        writer.Write("<p>\n");
        return new ParagraphWriterDisposable(writer);
    }
}

internal struct ParagraphWriterDisposable : IDisposable
{
    private readonly StreamWriter _writer;

    public ParagraphWriterDisposable(StreamWriter writer)
    {
        _writer = writer;
    }

    public void Dispose()
    {
        _writer?.Write("\n</p>\n");
    }
}