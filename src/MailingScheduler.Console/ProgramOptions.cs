using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace MailingScheduler.Console;

public class ProgramOptions
{
    public int MaxSendSpeed { get; set; }
    public int WorkIntervalMinutes { get; set; }
    public double UniformFraction { get; set; }
    public double PriorityFraction { get; set; }
    public double NonPriorityFraction { get; set; }
    public DateTime CurrentTime { get; set; }
}