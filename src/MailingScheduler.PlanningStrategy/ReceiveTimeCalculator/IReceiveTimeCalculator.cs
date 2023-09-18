using MailingScheduler.Core;

namespace MailingScheduler.PlanningStrategy.SendTimeCalculator;

public interface IReceiveTimeCalculator
{
    /// <summary>
    /// Рассчитать локальное время получения сообщения клиентом
    /// </summary>
    /// <param name="message">Сообщение, которое нужно отправить</param>
    /// <returns>Локальное время получения сообщения клиентом</returns>
    public DateTime CalculateReceiveTime(Message message);
}