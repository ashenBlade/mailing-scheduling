using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy.PriorityMessageChecker;
using MailingScheduler.PlanningStrategy.SendTimeCalculator;

namespace MailingScheduler.PlanningStrategy.PlanningStrategy;

public class PrioritizedPlanningStrategy: IPlanningStrategy
{
    private readonly IPriorityMessageChecker _priorityChecker;
    private readonly IReceiveTimeCalculator _receiveTimeCalculator;

    /// <summary>
    /// Максимальное количество приоритетных сообщений
    /// </summary>
    public int PriorityMax { get; }
    
    /// <summary>
    /// Максимальное количество неприоритетных сообщений
    /// </summary>
    public int NonPriorityMax { get; }

    public PrioritizedPlanningStrategy(int priorityMax, int nonPriorityMax, 
                                       IPriorityMessageChecker priorityChecker, IReceiveTimeCalculator receiveTimeCalculator)
    {
        _priorityChecker = priorityChecker;
        _receiveTimeCalculator = receiveTimeCalculator;
        PriorityMax = priorityMax;
        NonPriorityMax = nonPriorityMax;
    }
    
    public List<Message> Plan(List<Message> messages)
    {
        // Мы всегда берем не больше NonPriorityMax
        if (messages.Count <= NonPriorityMax)
        {
            return messages;
        }
        // Далее можем считать, что общее кол-во сообщений точно больше, чем NonPriorityMax
        
        // Разбиваем все сообщения на приоритетные и неприоритетные сообщения
        var prioritized = new List<MessageSendTime>();
        var nonPrioritized = new List<MessageSendTime>();
        foreach (var message in messages)
        {
            var messageSendTime = new MessageSendTime(message, _receiveTimeCalculator);
            if (_priorityChecker.IsPrioritized(message))
            {
                prioritized.Add(messageSendTime);
            }
            else
            {
                nonPrioritized.Add(messageSendTime);
            }
        }
        
        if (prioritized.Count == 0)
        {
            // Приоритетных сообщений больше NonPriorityMax - надо отсортировать и отобрать более приоритетные
            nonPrioritized.Sort(Comparer);
            return ToMessageList(nonPrioritized, NonPriorityMax);
        }
        
        // Приоритетные сообщения есть, но их меньше NonPriorityMax - надо добрать
        if (prioritized.Count <= NonPriorityMax)
        {
            // Кол-во неприоритетных больше 1, т.к. на старте проверили, что общая сумма не меньше NonPriorityMax
            var left = NonPriorityMax - prioritized.Count;
            nonPrioritized.Sort(Comparer);
            prioritized.AddRange(nonPrioritized.Take(left));
            return ToMessageList(prioritized);
        }
        
        // Теперь мы знаем, что приоритетных больше, чем NonPriorityMax
        
        // Если не достигаем своего предела максимума - вернуть все
        if (prioritized.Count <= PriorityMax)
        {
            return ToMessageList(prioritized);
        }
        
        prioritized.Sort(Comparer);
        return ToMessageList(prioritized, PriorityMax);
    }

    private static List<Message> ToMessageList(List<MessageSendTime> original, int toTake = -1)
    {
        List<Message> result;
        if (toTake == -1)
        {
            result = new List<Message>(original.Count);
            result.AddRange(original.Select(x => x.Message));
        }
        else
        {
            result = new List<Message>(toTake);
            result.AddRange(original.Select(x => x.Message).Take(toTake));
        }
        return result;
    }

    private struct MessageSendTime
    {
        public Message Message { get; set; }

        public DateTime SendTime => _sendTime ??= _receiveTimeCalculator.CalculateReceiveTime(Message);
        private DateTime? _sendTime;
        private readonly IReceiveTimeCalculator _receiveTimeCalculator;

        public MessageSendTime(Message message, IReceiveTimeCalculator calculator)
        {
            Message = message;
            _receiveTimeCalculator = calculator;
            _sendTime = null;
        }
    }

    private static readonly MessageSendTimeCalculatorComparer Comparer = new();
    private class MessageSendTimeCalculatorComparer : IComparer<MessageSendTime>
    {
        public int Compare(MessageSendTime x, MessageSendTime y)
        {
            return x.SendTime.CompareTo(y.SendTime);
        }
    }
}