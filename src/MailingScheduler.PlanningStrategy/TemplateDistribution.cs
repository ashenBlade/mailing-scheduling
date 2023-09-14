namespace MailingScheduler.PlanningStrategy;

public enum TemplateDistribution
{
    // Равномерное распределение
    Uniform = 0,
    
    // Утреннее
    Morning = 1,
    
    // Дневное 
    Daytime = 2,
    
    // Вечернее
    Evening = 3,
}