﻿namespace MailingScheduler.Core;

public record Message(Guid Id, string TemplateCode, DateTime StartTime, DateTime EndTime, TimeSpan ClientTimezoneOffset);