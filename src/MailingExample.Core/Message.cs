namespace MailingExample.Core;

public record Message(Guid Id, string TemplateCode, DateTime StartTime, DateTime EndTime, int ClientTimezoneOffset);