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
        WorkIntervalMinutes = 2,
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

    var (templates, messages) = LoadData(programOptions);

    var maxToSend = programOptions.MaxSendSpeed * programOptions.WorkIntervalMinutes;
    var scheduler = new StrategyMailingScheduler(templates, maxToSend);

    Log.Information("Начинаю планирование сообщений");
    var scheduleWatch = Stopwatch.StartNew();
    var scheduled = scheduler.Schedule(messages)
                             .ToList();
    scheduleWatch.Stop();
    var elapsed = scheduleWatch.Elapsed;
    Log.Debug("Время работы: {WorkTime}", elapsed);
    totalTimeWatcher.Stop();
    var statistics = new StatisticsCalculator(templates).CalculateStatistics(scheduled, totalTimeWatcher.Elapsed, scheduleWatch.Elapsed);
    
    var saver = new HtmlStatisticsSaver("statistics.html");
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

( Template[], Message[] ) LoadData(ProgramOptions options)
{
    var templateGenerateInfos = new TemplateGenerateInfo[]
    {
        new(new("Sample", Priority.High, TemplateDistribution.Daytime, 1000), 2345, 200 ),
        new( new("Another", Priority.Normal, TemplateDistribution.Evening, 1000), 15442, 2132 ),
        new( new("Hello", Priority.Normal, TemplateDistribution.Uniform, 1000), 1256, null ),
        new( new("World", Priority.Low, TemplateDistribution.Uniform, 1000), 9888, null ),
        new( new("aaaaa", Priority.Normal, TemplateDistribution.Morning, 1500), 10023, 4000 ),
    };
    var messages = MessagesGenerator
                  .GenerateMessages(options.CurrentTime, templateGenerateInfos)
                  .ToArray();
    
    var templateFactory = TemplateFactory.Create(templateGenerateInfos.Length,
        options.PriorityFraction, options.NonPriorityFraction,
        options.UniformFraction, options.MaxSendSpeed, options.WorkIntervalMinutes, 
        new LocalTimeReceiveTimeCalculator(options.CurrentTime));
    var templates = templateGenerateInfos.Select(x => templateFactory.CreateTemplate(x.TemplateInfo))
                                         .ToArray();
    return ( templates, messages );
    
    // Log.Information("Создаю Data Source для БД");
    // var dbContextOptions = new DbContextOptionsBuilder<MailingDbContext>()
    //                       .UseNpgsql(options.ConnectionString)
    //                       .Options;
    //
    //
    // MailingScheduler.Database.Message[] databaseMessages;
    // HashSet<string> templateCodes;
    // MailingScheduler.Database.Template[] databaseTemplates;
    // using (var context = new MailingDbContext(dbContextOptions))
    // {
    //     Log.Information("Загружаю {MaxMessages} сообщений", options.MaxToFetch);
    //     databaseMessages = context.Messages
    //                               .Take(options.MaxToFetch)
    //                               .ToArray();
    //     Log.Information("Загружено {LoadedCount} сообщений", databaseMessages.Length);
    //     templateCodes = databaseMessages
    //                    .Select(x => x.TemplateCode)
    //                    .ToHashSet();
    //     
    //     Log.Information("Загружаю {TemplatesCount} шаблонов", templateCodes.Count);
    //     databaseTemplates = context.Templates
    //                                .Where(x => templateCodes.Contains(x.TemplateCode))
    //                                .ToArray();
    //     Log.Information("Шаблоны загружены");
    // }
    //
    // Log.Debug("Начинаю конвертировать объекты БД в доменные");
    // var templateFactory = TemplateFactory.Create(templateCodes.Count,
    //                                              options.PriorityFraction, options.NonPriorityFraction,
    //                                              options.UniformFraction, options.MaxSendSpeed, options.WorkInterval, 
    //                                              new LocalTimeReceiveTimeCalculator(DateTime.Now));
    //
    // var t = Array.ConvertAll(databaseTemplates, t =>
    // {
    //     var priority = ( Priority ) (( int ) t.Priority);
    //     var distribution = ( TemplateDistribution ) (( int ) t.Distribution);
    //     return templateFactory.CreateTemplate(new TemplateInfo(t.TemplateCode, priority, distribution, t.MaxSendSpeed));
    // });
    // var m = Array.ConvertAll(databaseMessages, m =>
    // {
    //     return new Message(m.Id, m.TemplateCode, m.StartTime, m.EndTime, m.ClientTimezoneOffset);
    // });
    // return (t, m);
}