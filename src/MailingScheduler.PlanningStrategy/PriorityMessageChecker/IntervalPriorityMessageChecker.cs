using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy.SendTimeCalculator;

namespace MailingScheduler.PlanningStrategy.PriorityMessageChecker;

/// <summary>
/// Сообщение приоритетное, если время его получения попадает в приоритетный интервал
/// </summary>
public class IntervalPriorityMessageChecker: IPriorityMessageChecker
{
    /// <summary>
    /// Начало приоритетного интервала
    /// </summary>
    private readonly TimeOnly _startTime;
    
    /// <summary>
    /// Конец приоритетного интервала
    /// </summary>
    private readonly TimeOnly _endTime;
    private readonly IReceiveTimeCalculator _receiveTimeCalculator;
    
    public IntervalPriorityMessageChecker(TimeOnly startTime, TimeOnly endTime, IReceiveTimeCalculator receiveTimeCalculator)
    {
        _startTime = startTime;
        _endTime = endTime;
        _receiveTimeCalculator = receiveTimeCalculator;
    }
    
    public bool IsPrioritized(Message message)
    {
        // Находится ли время получения сообщения в приоритетном интервале
        var receiveTime = TimeOnly.FromDateTime(_receiveTimeCalculator.CalculateReceiveTime(message));
        return _startTime <= receiveTime && receiveTime <= _endTime;
    }
}