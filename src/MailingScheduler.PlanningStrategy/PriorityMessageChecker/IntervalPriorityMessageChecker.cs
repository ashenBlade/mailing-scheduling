using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy.SendTimeCalculator;

namespace MailingScheduler.PlanningStrategy.PriorityMessageChecker;

public class IntervalPriorityMessageChecker: IPriorityMessageChecker
{
    private readonly IReceiveTimeCalculator _receiveTimeCalculator;
    /// <summary>
    /// Начало приоритетного интервала
    /// </summary>
    private readonly TimeOnly _startTime;
    
    /// <summary>
    /// Конец приоритетного интервала
    /// </summary>
    private readonly TimeOnly _endTime;

    public IntervalPriorityMessageChecker(IReceiveTimeCalculator receiveTimeCalculator, TimeOnly startTime, TimeOnly endTime)
    {
        _receiveTimeCalculator = receiveTimeCalculator;
        _startTime = startTime;
        _endTime = endTime;
    }
    
    public bool IsPrioritized(Message message)
    {
        var receiveTime = TimeOnly.FromDateTime(_receiveTimeCalculator.CalculateReceiveTime(message));
        return _startTime <= receiveTime && receiveTime <= _endTime;
    }
}