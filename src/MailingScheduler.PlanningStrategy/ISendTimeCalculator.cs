using MailingScheduler.Core;

namespace MailingScheduler.PlanningStrategy;

public interface ISendTimeCalculator
{
    public DateTime CalculateSendTime(Message message);
}