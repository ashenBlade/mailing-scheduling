namespace MailingScheduler.Core;

public interface IPlanningStrategy
{
    /// <summary>
    /// Отобрать только нужные сообщения в соответствии с алгоритмом планирования
    /// </summary>
    /// <param name="messages">Сообщения, из которых нужно отобрать нужные</param>
    /// <returns>Отобранные сообщения</returns>
    /// <remarks>
    /// Передаваемый массив может модифицироваться.
    /// Возвращаться может тот же самый массив, что и переданный (ссылка та же)
    /// </remarks>
    public List<Message> Plan(List<Message> messages);
}