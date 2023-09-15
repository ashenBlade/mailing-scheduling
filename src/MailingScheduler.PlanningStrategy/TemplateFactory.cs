using System.ComponentModel;
using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy.PlanningStrategy;
using MailingScheduler.PlanningStrategy.PriorityMessageChecker;
using MailingScheduler.PlanningStrategy.SendTimeCalculator;

namespace MailingScheduler.PlanningStrategy;

public class TemplateFactory
{
    /// <summary>
    /// Максимальное кол-во НЕ приоритетных сообщений, без учета скорости шаблона 
    /// </summary>
    private readonly int _nonPriorityOriginalMax;
    
    /// <summary>
    /// Максимальное кол-во приоритетных сообщений, без учета скорости шаблона
    /// </summary>
    private readonly int _priorityOriginalMax;
    
    /// <summary>
    /// Максимальное кол-во сообщений при равномерном распределении, без учета скорости шаблона
    /// </summary>
    private readonly int _uniformOriginalMax;
    
    /// <summary>
    /// Интервал работы в минутах
    /// </summary>
    private readonly int _intervalMinutes;
    
    private readonly IntervalPriorityMessageChecker _morningInterval;
    private readonly IntervalPriorityMessageChecker _daytimeInterval;
    private readonly IntervalPriorityMessageChecker _eveningInterval;

    private readonly IReceiveTimeCalculator _calculator;
    private TemplateFactory(int nonPriorityOriginalMax, int priorityOriginalMax, int uniformOriginalMax, int intervalMinutes, IReceiveTimeCalculator calculator)
    {
        _nonPriorityOriginalMax = nonPriorityOriginalMax;
        _priorityOriginalMax = priorityOriginalMax;
        _uniformOriginalMax = uniformOriginalMax;
        _intervalMinutes = intervalMinutes;
        _calculator = calculator;
        
        _morningInterval = new IntervalPriorityMessageChecker(new TimeOnly(0, 0), new TimeOnly(12, 0), calculator);
        _daytimeInterval = new IntervalPriorityMessageChecker(new TimeOnly(12, 0), new TimeOnly(17, 0), calculator);
        _eveningInterval = new IntervalPriorityMessageChecker(new TimeOnly(17, 0), new TimeOnly(23, 59, 59), calculator);
    }

    public Template CreateTemplate(TemplateInfo info)
    {
        return new Template(info.TemplateCode, info.Priority, CreatePlanningStrategy(info));
    }
    
    private IPlanningStrategy CreatePlanningStrategy(TemplateInfo info)
    {
        if (info.Distribution is TemplateDistribution.Uniform)
        {
            return CreateUniformPlanningStrategy(info);
        }
        return CreatePrioritizedPlanningStrategy(info);
    }
    private PrioritizedPlanningStrategy CreatePrioritizedPlanningStrategy(TemplateInfo info)
    {
        var templateMaxMessages = info.CalculateMaxMessagesForInterval(_intervalMinutes);
        
        var priorityMax = Math.Max(Math.Min(templateMaxMessages, _priorityOriginalMax), 1);
        var nonPriorityMax = Math.Max(Math.Min(templateMaxMessages, _nonPriorityOriginalMax), 1);

        
        
        var priorityChecker = info.Distribution switch
                              {
                                TemplateDistribution.Morning => _morningInterval,
                                TemplateDistribution.Daytime => _daytimeInterval,
                                TemplateDistribution.Evening => _eveningInterval,
                                TemplateDistribution.Uniform => throw new InvalidOperationException($"У равномерного распределения нет приоритетного интервала. Ошибка у шаблона {info.TemplateCode}"),
                                _                            => throw new InvalidEnumArgumentException(nameof(info.Distribution), (int)info.Distribution, typeof(TemplateDistribution))
                              };

        return new PrioritizedPlanningStrategy(priorityMax, nonPriorityMax, info.Distribution, priorityChecker, _calculator);
    }
    
    private UniformPlanningStrategy CreateUniformPlanningStrategy(TemplateInfo info)
    {
        var messagesMax = Math.Max(Math.Min(info.CalculateMaxMessagesForInterval(_intervalMinutes), _uniformOriginalMax), 1);
        return new UniformPlanningStrategy(messagesMax);
    }

    public static TemplateFactory Create(int differentTemplatesCount, 
                                         double priorityFraction, 
                                         double nonPriorityFraction, double uniformFraction,
                                         int maxSendSpeedPerMinute,
                                         int intervalMinutes,
                                         IReceiveTimeCalculator calculator)
    {
        // Корректируем доли шаблонов на случай, если шаблонов будет мало
        var adjustedFraction = 1d / differentTemplatesCount;
        nonPriorityFraction = Math.Max(nonPriorityFraction, adjustedFraction);
        priorityFraction = Math.Max(priorityFraction, adjustedFraction);
        uniformFraction = Math.Max(uniformFraction, adjustedFraction);

        // Вычисляем максимальное кол-во сообщений, которое можем отправить в общем
        var maxToSend = maxSendSpeedPerMinute * intervalMinutes;
        
        // Вычисляем границы для приоритезированного распределения
        var nonPriorityOriginalMax = ( int ) (nonPriorityFraction * maxToSend);
        var priorityOriginalMax = ( int ) (priorityFraction * maxToSend);
        
        // Вычисляем границу для равномерного распределения
        var uniformOriginalMax = ( int ) (uniformFraction * maxToSend);
        
        return new TemplateFactory(nonPriorityOriginalMax, priorityOriginalMax, uniformOriginalMax, intervalMinutes, calculator);
    }
}

file static class TemplateInfoExtensions
{
    public static int CalculateMaxMessagesForInterval(this TemplateInfo templateInfo, int intervalMinutes)
    {
        return templateInfo.MaxSendSpeed * intervalMinutes;
    }
}