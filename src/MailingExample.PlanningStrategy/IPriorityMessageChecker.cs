using MailingExample.Core;

namespace MailingExample.PlanningStrategy;

public interface IPriorityMessageChecker
{
    /// <summary>
    /// Является ли переданное сообщение приоритетным
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <returns><c>true</c> - сообщение приоритетное, <c>false</c> - сообщение неприоритетное</returns>
    public bool IsPrioritized(Message message);
}