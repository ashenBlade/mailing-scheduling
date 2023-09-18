using MailingScheduler.Core;

namespace MailingScheduler.PlanningStrategy.SendTimeCalculator;

public class LocalTimeReceiveTimeCalculator: IReceiveTimeCalculator
{
    private readonly DateTime _currentTime;
    public LocalTimeReceiveTimeCalculator(DateTime currentTime)
    {
        _currentTime = currentTime;

    }
    public DateTime CalculateReceiveTime(Message message)
    {
        return _currentTime - TimeSpan.FromHours( message.ClientTimezoneOffset );
    }
}