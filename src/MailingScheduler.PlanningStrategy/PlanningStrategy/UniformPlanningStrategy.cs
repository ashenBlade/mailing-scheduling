using System.Diagnostics;
using MailingScheduler.Core;

namespace MailingScheduler.PlanningStrategy.PlanningStrategy;

public class UniformPlanningStrategy: IPlanningStrategy
{
    /// <summary>
    /// Максимальное количество сообщений, которое можем отобрать
    /// </summary>
    private int Max { get; }

    public UniformPlanningStrategy(int max)
    {
        Max = max;
    }

    public List<Message> Plan(List<Message> messages)
    {
        if (messages.Count <= Max)
        {
            // Общее число сообщений меньше предела - возвращаем все
            return messages;
        }
        
        messages.Sort(Comparer);
        // Ограничиваем размер до Max
        messages.RemoveRange(Max, messages.Count - Max);
        return messages;
    }

    private static readonly StartEndSendTimeMessageComparer Comparer = new();
    private class StartEndSendTimeMessageComparer : IComparer<Message>
    {
        public int Compare(Message? x, Message? y)
        {
            // Сравнение по кортежу (Дата отправки, Дата окончания)
            Debug.Assert(x is not null);
            Debug.Assert(y is not null);
            var startGreater = x.StartTime.CompareTo(y.StartTime);
            if (startGreater == 0)
            {
                return x.EndTime.CompareTo(y.EndTime);
            }

            return startGreater;
        }
    }
}