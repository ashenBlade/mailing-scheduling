// See https://aka.ms/new-console-template for more information


using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MailingScheduler.Console;
using MailingScheduler.Core;
using MailingScheduler.Database;
using MailingScheduler.PlanningStrategy;
using MailingScheduler.PlanningStrategy.SendTimeCalculator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Serilog;
using Message = MailingScheduler.Core.Message;
using Template = MailingScheduler.Core.Template;

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

var configuration = new ConfigurationBuilder()
                   .AddEnvironmentVariables()
                   .Build();

var programOptions = configuration.Get<ProgramOptions>()!;
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

var maxToSend = programOptions.MaxSendSpeed * programOptions.WorkInterval;
var scheduler = new StrategyMailingScheduler(templates, maxToSend);

Log.Information("Начинаю планирование сообщений");
var watch = Stopwatch.StartNew();
var scheduled = scheduler.Schedule(messages)
                         .ToList();
watch.Stop();
var elapsed = watch.Elapsed;
Log.Information("Время работы: {WorkTime}", elapsed);


( Template[], Message[] ) LoadData(ProgramOptions options)
{
    Log.Information("Создаю Data Source для БД");
    var dbContextOptions = new DbContextOptionsBuilder<MailingDbContext>()
                          .UseNpgsql(options.ConnectionString)
                          .Options;


    MailingScheduler.Database.Message[] databaseMessages;
    HashSet<string> templateCodes;
    MailingScheduler.Database.Template[] databaseTemplates;
    using (var context = new MailingDbContext(dbContextOptions))
    {
        Log.Information("Загружаю {MaxMessages} сообщений");
        databaseMessages = context.Messages
                                  .Take(options.MaxToFetch)
                                  .ToArray();
        Log.Information("Загружено {LoadedCount} сообщений", databaseMessages.Count);
        templateCodes = databaseMessages
                       .Select(x => x.TemplateCode)
                       .ToHashSet();
        
        Log.Information("Загружаю {TemplatesCount} шаблонов", templateCodes.Count);
        databaseTemplates = context.Templates
                                   .Where(x => templateCodes.Contains(x.TemplateCode))
                                   .ToArray();
        Log.Information("Шаблоны загружены");
    }

    Log.Debug("Начинаю конвертировать объекты БД в доменные");
    var templateFactory = TemplateFactory.Create(templateCodes.Count,
                                                 options.PriorityFraction, options.NonPriorityFraction,
                                                 options.UniformFraction, options.MaxSendSpeed, options.WorkInterval, 
                                                 new LocalTimeReceiveTimeCalculator(DateTime.Now));

    var templates = Array.ConvertAll(databaseTemplates, t =>
    {
        var priority = ( Priority ) (( int ) t.Priority);
        var distribution = ( TemplateDistribution ) (( int ) t.Distribution);
        return templateFactory.CreateTemplate(new TemplateInfo(t.TemplateCode, priority, distribution, t.MaxSendSpeed));
    });
    var messages = Array.ConvertAll(databaseMessages, m =>
    {
        return new Message(m.Id, m.TemplateCode, m.StartTime, m.EndTime, TimeSpan.FromHours(m.ClientTimezoneOffset));
    });
    return (templates, messages);
}