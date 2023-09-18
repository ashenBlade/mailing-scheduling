using MailingScheduler.Core;
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

        // 2. Кол-во различных шаблонов
        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Количество различных шаблонов: {0}", statistics.DifferentTemplatesCount);
        }
        
        // 3. Кол-во сообщений по приоритетам
        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Количество взятых сообщений по приоритетам");
            writer.Write("<table>");
            writer.Write("<thead><tr><th>Приоритет</th><th>Число сообщений</th></tr></thead>");
            writer.Write("<tbody>");
            foreach (var priority in Enum.GetValues<Priority>())
            {
                var count = statistics.PriorityMessagesCount[priority];
                writer.Write("<tr><td>{0}</td><td>{1}</td></tr>", priority.ToString(), count);
            }
            writer.Write("</tbody>");
            writer.Write("</table>");
        }
        
        // 4. Таблица со всем данными
        using (var _ = writer.BeginParagraph())
        {
            writer.Write("Общая сводка");
            writer.Write("<table>");
            writer.Write("<thead><tr><th>Шаблон</th><th>Приоритет</th><th>Общее число сообщений</th><th>Распределение</th><th>Число приоритетных</th><th>Число неприоритетных</th></tr></thead>");
            writer.Write("<tbody>");
            foreach (var group in statistics.TemplateGroups)
            {
                var summary = group.GetSummary();
                string priorityCount = "";
                string nonPriorityCount = "";
                if (summary.PriorityMessagesCount is {} priorityMessagesCount)
                {
                    priorityCount = priorityMessagesCount.ToString();
                    nonPriorityCount = ( summary.TotalMessagesCount - priorityMessagesCount ).ToString();
                }
                writer.Write("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>",
                    summary.TemplateCode, summary.Priority.ToString(), summary.TotalMessagesCount, summary.Distribution.ToString(), priorityCount, nonPriorityCount);
            }
            writer.Write("</tbody>");
            writer.Write("</table>");
        }
    }
}

internal static class StreamWriterExtensions
{
    public static ParagraphWriterDisposable BeginParagraph(this StreamWriter writer)
    {
        writer.Write("<p>");
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
        _writer?.Write("</p>");
    }
}