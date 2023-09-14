using MailingExample.Core;

namespace MailingExample.PlanningStrategy;

public interface ISendTimeCalculator
{
    public DateTime CalculateSendTime(Message message);
}