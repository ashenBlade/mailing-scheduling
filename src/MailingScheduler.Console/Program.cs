// See https://aka.ms/new-console-template for more information


using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MailingScheduler.Console;
using MailingScheduler.Core;
using MailingScheduler.Generator;
using MailingScheduler.PlanningStrategy;
using MailingScheduler.PlanningStrategy.SendTimeCalculator;
using MailingScheduler.Statistics;
using Serilog;
using Message = MailingScheduler.Core.Message;
using Template = MailingScheduler.Core.Template;

var totalTimeWatcher = Stopwatch.StartNew();
Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

try
{
    var programOptions = new ProgramOptions()
    {
        WorkIntervalMinutes = 5,
        MaxSendSpeed = 10000,
        UniformFraction = 0.03,
        PriorityFraction = 0.05,
        NonPriorityFraction = 0.01,
        CurrentTime = new DateTime(2023, 9, 18, 14, 0, 0)
    };
    
    try
    {
        Validator.ValidateObject(programOptions, new ValidationContext(programOptions), true);
    }
    catch (Exception e)
    {
        Log.Fatal(e, "Ошибка во время валидации конфигурации");
        throw;
    }

    var (templates, messages) = GenerateData(programOptions);

    var maxToSend = programOptions.MaxSendSpeed * programOptions.WorkIntervalMinutes;
    Log.Debug("Максимальное число сообщений к отправке: {MaxCount}", maxToSend);
    var scheduler = new StrategyMailingScheduler(templates, maxToSend);

    Log.Information("Начинаю планирование");
    var scheduleWatch = Stopwatch.StartNew();
    var scheduled = scheduler.Schedule(messages)
                             .ToList();
    scheduleWatch.Stop();
    var elapsed = scheduleWatch.Elapsed;
    
    Log.Debug("Время работы: {WorkTime}", elapsed);
    totalTimeWatcher.Stop();
    
    Log.Information("Рассчитываю статистику");
    var statistics = new StatisticsCalculator(templates, messages)
       .CalculateStatistics(scheduled, totalTimeWatcher.Elapsed, scheduleWatch.Elapsed);
    var saver = new HtmlStatisticsSaver("statistics.html");
    Log.Information("Сохраняю статистику");
    saver.SaveStatistics(statistics);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Необработанное исключение во время планирования сообщений");
}
finally
{
    Log.CloseAndFlush();
}

return;

static ( Template[], Message[] ) GenerateData(ProgramOptions options)
{
    var templateInfos = TemplateInfoGenerator
       .GenerateRandomTemplateInfos(100);
    
    var messages = MessagesGenerator
                  .GenerateMessages(options.CurrentTime, templateInfos)
                  .ToArray();
    
    var templateFactory = TemplateFactory.Create(
        templateInfos.Length,
        options.PriorityFraction, options.NonPriorityFraction,
        options.UniformFraction, options.MaxSendSpeed, options.WorkIntervalMinutes, 
        new LocalTimeReceiveTimeCalculator(options.CurrentTime));
    
    var templates = templateFactory.MapToTemplates(templateInfos);
    
    return ( templates, messages );
}