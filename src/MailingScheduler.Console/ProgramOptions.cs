using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace MailingScheduler.Console;

public class ProgramOptions
{
    /// <summary>
    /// Строка подключения к БД
    /// </summary>
    [ConfigurationKeyName("CONNECTION_STRING")]
    [Required]
    public string ConnectionString { get; set; } = null!;
    
    /// <summary>
    /// Максимальное кол-во сообщений, которое можно загрузить за раз
    /// </summary>
    [ConfigurationKeyName("MAX_TO_FETCH")]
    [Range(1, int.MaxValue)]
    public int MaxToFetch { get; set; }
    
    /// <summary>
    /// Максимальная скорость отправки сообщений в минуту.
    /// Значение из общего рейт лимитера
    /// </summary>
    [ConfigurationKeyName("MAX_SEND_SPEED")]
    [Range(1, int.MaxValue)]
    public int MaxSendSpeed { get; set; }

    /// <summary>
    /// Интервал работы планировщика
    /// </summary>
    [ConfigurationKeyName("WORK_INTERVAL")]
    [Range(1, int.MaxValue)]
    public int WorkInterval { get; set; }
    
    [ConfigurationKeyName("UNIFORM_FRACTION")]
    [Range(0.00000001, 1)]
    public double UniformFraction { get; set; }
    
    [ConfigurationKeyName("PRIORITY_FRACTION")]
    [Range(0.00000001, 1)]
    public double PriorityFraction { get; set; }
    
    [ConfigurationKeyName("NON_PRIORITY_FRACTION")]
    [Range(0.00000001, 1)]
    public double NonPriorityFraction { get; set; }
}