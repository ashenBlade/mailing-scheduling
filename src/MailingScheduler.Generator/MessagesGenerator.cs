using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy;

namespace MailingScheduler.Generator;

public static class MessagesGenerator
{
    public static IEnumerable<Message> GenerateMessages(DateTime currentTime, IEnumerable<TemplateGenerateInfo> generateInfos)
    {
        var calculator = new ClientTimezoneOffsetCalculator(currentTime);
        var startTime = currentTime - TimeSpan.FromDays(1);
        var endTime = currentTime + TimeSpan.FromDays(1);
        foreach (var generateInfo in generateInfos)
        {
            // Для каждого шаблона генерируем нужное количество сообщений
            if (generateInfo.TemplateInfo.Distribution is TemplateDistribution.Uniform)
            {
                for (int i = 0; i < generateInfo.TotalMessagesCount; i++)
                {
                    yield return new Message(Guid.NewGuid(), generateInfo.TemplateInfo.TemplateCode, startTime, endTime,
                        calculator.GetClientTimezoneOffsetForUniform());
                }
                continue;
            }

            var priorityCount = generateInfo.PriorityCount ?? 0;
            if (priorityCount > 0)
            {
                for (int i = 0; i < priorityCount; i++)
                {
                    yield return new Message(Guid.NewGuid(), generateInfo.TemplateInfo.TemplateCode, startTime, endTime,
                        calculator.GetClientTimezoneOffsetForPriority(generateInfo.TemplateInfo.Distribution));
                }
            }

            var left = generateInfo.TotalMessagesCount - priorityCount;
            for (int i = 0; i < left; i++)
            {
                yield return new Message(Guid.NewGuid(), generateInfo.TemplateInfo.TemplateCode, startTime, endTime,
                    calculator.GetClientTimezoneOffsetForNonPriority(generateInfo.TemplateInfo.Distribution));
            }
        }
    }
}