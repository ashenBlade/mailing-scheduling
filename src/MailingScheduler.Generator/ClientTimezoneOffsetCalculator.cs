using System.ComponentModel;
using MailingScheduler.PlanningStrategy;

namespace MailingScheduler.Generator;

internal class ClientTimezoneOffsetCalculator
{
    private readonly Random _random = new();

    public ClientTimezoneOffsetCalculator(DateTime currentDateTime)
    {
        _morningPriorityOffsets = CalculateOffsetsToSuitInterval(currentDateTime, Morning);
        _daytimePriorityOffsets = CalculateOffsetsToSuitInterval(currentDateTime, Daytime);
        _eveningPriorityOffsets = CalculateOffsetsToSuitInterval(currentDateTime, Evening);

        var offsetsInterval = ( -2, 8 );
        _morningNonPriorityOffsets = GetNumbersBetweenExcluding(offsetsInterval, _morningPriorityOffsets);
        _daytimeNonPriorityOffsets = GetNumbersBetweenExcluding(offsetsInterval, _daytimePriorityOffsets);
        _eveningNonPriorityOffsets = GetNumbersBetweenExcluding(offsetsInterval, _eveningPriorityOffsets);
    }
    
    private readonly (int Start, int End) _morningPriorityOffsets;
    private readonly (int Start, int End) _daytimePriorityOffsets;
    private readonly (int Start, int End) _eveningPriorityOffsets;

    private readonly int[] _morningNonPriorityOffsets;
    private readonly int[] _daytimeNonPriorityOffsets;
    private readonly int[] _eveningNonPriorityOffsets;
    
    
    private static int[] GetNumbersBetweenExcluding((int Start, int End) includeInterval, (int Start, int End) excludeInterval)
    {
        var result = new List<int>(includeInterval.End - includeInterval.Start);
        for (int i = includeInterval.Start; i <= includeInterval.End; i++)
        {
            if (InInterval(i))
            {
                continue;
            }
            
            result.Add(i);
        }

        return result.ToArray();
        
        bool InInterval(int value)
        {
            return excludeInterval.Start <= value && value < excludeInterval.End;
        }
    }
    
    private static readonly (TimeOnly Start, TimeOnly End) Morning = ( new TimeOnly(0, 0), new TimeOnly(12, 00) );
    private static readonly (TimeOnly Start, TimeOnly End) Daytime = ( new TimeOnly(12, 0), new TimeOnly(17, 00) );
    private static readonly (TimeOnly Start, TimeOnly End) Evening = ( new TimeOnly(17, 0), new TimeOnly(23, 59, 59) );

    /// <summary>
    /// Найти временную зону, такую чтобы время получения сообщения клиентом было приоритетным
    /// </summary>
    /// <param name="distribution">Распределение шаблона</param>
    /// <returns>Временная зона клиента</returns>
    public int GetClientTimezoneOffsetForPriority(TemplateDistribution distribution)
    {
        var (start, end) = distribution switch
                           {
                               TemplateDistribution.Morning => _morningPriorityOffsets,
                               TemplateDistribution.Daytime => _daytimePriorityOffsets,
                               TemplateDistribution.Evening => _eveningPriorityOffsets,
                               _ => throw new InvalidEnumArgumentException(nameof(distribution), ( int ) distribution,
                                        typeof(TemplateDistribution))
                           };
        return _random.Next(start, end);
    }

    /// <summary>
    /// Найти временную зону, при котором получение сообщения будет НЕ в приоритетном интервале
    /// </summary>
    /// <param name="distribution">Распределение шаблона</param>
    /// <returns>Временная зона клиента</returns>
    public int GetClientTimezoneOffsetForNonPriority(TemplateDistribution distribution)
    {
        var offsets = distribution switch
                      {
                          TemplateDistribution.Daytime => _daytimeNonPriorityOffsets,
                          TemplateDistribution.Morning => _morningNonPriorityOffsets,
                          TemplateDistribution.Evening => _eveningNonPriorityOffsets,
                          _ => throw new InvalidEnumArgumentException(nameof(distribution), ( int ) distribution,
                                   typeof(TemplateDistribution))
                      };
        return offsets[_random.Next(0, offsets.Length)];
    }

    public int GetClientTimezoneOffsetForUniform()
    {
        return _random.Next(-2, 8);
    }

    private static (int Start, int End) CalculateOffsetsToSuitInterval(DateTime currentDateTime, (TimeOnly Start, TimeOnly End) interval)
    {
        var currentTime = TimeOnly.FromDateTime(currentDateTime);
        var (start, end) = interval;
        
        if (start <= currentTime && currentTime <= end)
        {
            // Текущее время находится в приоритетном интервале
            var backwardDelta = currentTime - start;
            var forwardDelta = end - currentTime;
            return ( ( int ) backwardDelta.TotalHours, ( ( int ) forwardDelta.TotalHours ) + 1 );
        }
        
        if (currentTime <= start)
        {
            // Приоритетный интервал будет дальше
            var forwardDelta = start - currentTime;
            var deltaBetween = ( end - start );

            return ( ( int ) forwardDelta.TotalHours, ( int )deltaBetween.TotalHours + 1 );
        }
        else /* end <= currentTime */
        {
            var backwardDelta = - ( int ) ( start - currentTime ).TotalHours;
            var deltaBetween = backwardDelta + ( int ) ( end - start ).TotalHours;
            return ( backwardDelta, deltaBetween + 1 );
        }
    }
}