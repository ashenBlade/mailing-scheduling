namespace MailingScheduler.PlanningStrategy;

public enum TemplateDistribution
{
    // Равномерное
    Uniform = 0,
    // Утреннее: до 12:00
    Morning = 1,
    // Дневное: до 17:00
    Daytime = 2,
    // Вечернее: до полночи
    Evening = 3,
}