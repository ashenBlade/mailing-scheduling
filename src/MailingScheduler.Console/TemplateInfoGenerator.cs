using MailingScheduler.Core;
using MailingScheduler.Database;
using MailingScheduler.PlanningStrategy;

namespace MailingScheduler.Console;

public static class TemplateInfoGenerator
{
    public static TemplateInfo[] GenerateRandomTemplateInfos(int templatesCount)
    {
        var random = new Random();
        var result = new TemplateInfo[templatesCount];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = new TemplateInfo(
                i.ToString(),
                GetRandomPriority(),
                GetRandomDistribution(),
                GetRandomTemplateSpeed());
        }
        
        return result;
        
        Priority GetRandomPriority() => ( Priority ) random.Next(0, 4);
        TemplateDistribution GetRandomDistribution() => ( TemplateDistribution ) random.Next(0, 4);
        int GetRandomTemplateSpeed() => random.Next(1, 4) * 1000;
    }

}