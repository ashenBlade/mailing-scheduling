namespace MailingScheduler.Core;

public class MailingScheduler
{
    private readonly Template[] _templates;
    
    /// <summary>
    /// Максимальное число сообщений, которое можно отправить
    /// </summary>
    public int MaxToSend { get; }

    public MailingScheduler(Template[] templates, int maxToSend)
    {
        _templates = templates;
        MaxToSend = maxToSend;
    }
    
    public IEnumerable<Message> Schedule(IEnumerable<Message> messages)
    {
        var priorityGroups = PriorityMessageGroups.Build(messages, _templates);
        var left = MaxToSend;
        foreach (var templateGroups in priorityGroups.GetPriorityGroups())
        {
            var planned = templateGroups.Plan();
            foreach (var message in planned)
            {
                yield return message;
                left--;
            }

            if (left == 0)
            {
                yield break;
            }
        }
    }


    public class TemplateMessageGroups
    {
        private readonly IEnumerable<(Template Template, List<Message> Messages)> _groups;

        public TemplateMessageGroups(IEnumerable<(Template Template, List<Message> Messages)> groups)
        {
            _groups = groups;
        }
        
        public Message[] Plan()
        {
            var groups = _groups.ToArray();
            var templateGroups = Array.ConvertAll(groups, g => new TemplateMessageGroup(g.Template, g.Messages));
            // Планируем в каждой группе
            Parallel.ForEach(templateGroups, g => g.Schedule());

            // Объединяем все сообщения в одной группе
            var totalSize = templateGroups.Sum(x => x.Result.Count);
            var result = new Message[totalSize];
            var index = 0;
            for (var i = 0; i < templateGroups.Length; i++)
            {
                var re = templateGroups[i].Result;
                re.CopyTo(result, index);
                index += re.Count;
            }
            
            // Сортируем по датам
            Array.Sort(result, static (left, right) =>
            {
                // Сравнение по кортежу (дата начала отправки, дата окончания отправки)
                var startTimeCompareResult = DateTime.Compare(left.StartTime, right.StartTime);
                return startTimeCompareResult == 0
                           ? DateTime.Compare(left.EndTime, right.EndTime)
                           : startTimeCompareResult;
            });

            return result;
        }

        public static TemplateMessageGroups Build(List<(Template Template, Message Message)> messages)
        {
            var result = new Dictionary<Template, List<Message>>(Template.TemplateCodeComparer);
            
            foreach (var (template, message) in messages)
            {
                if (result.TryGetValue(template, out var list))
                {
                    list.Add(message);
                }
                else
                {
                    result[template] = new List<Message>(1) {message};
                }
            }
            
            return new TemplateMessageGroups(result.Select(x => ( x.Key, x.Value )));
        }
        
        private class TemplateMessageGroup
        {
            private Template Template { get; }
            private List<Message> Messages { get; }
        
            public TemplateMessageGroup(Template template, List<Message> messages)
            {
                Template = template;
                Messages = messages;
            }

            public List<Message> Result { get; private set; } = null!;
            public void Schedule()
            {
                Result = Template.Strategy.Plan(Messages);
            }
        }
    }
    
    private class PriorityMessageGroups
    {
        private readonly List<List<(Template Template, Message Message)>> _priorityGroups;
        
        private PriorityMessageGroups(List<List<(Template Template, Message Message)>> priorityGroups)
        {
            _priorityGroups = priorityGroups;
        }

        public IEnumerable<TemplateMessageGroups> GetPriorityGroups()
        {
            foreach (var priorityGroupMessages in _priorityGroups)
            {
                yield return TemplateMessageGroups.Build(priorityGroupMessages);
            }
        }

        public static PriorityMessageGroups Build(IEnumerable<Message> messages, IEnumerable<Template> templates)
        {
            var result = new List<List<(Template, Message)>>
            {
                new(), // Realtime
                new(), // High
                new(), // Normal
                new(), // Low
            };
            var codeTemplate = templates.ToDictionary(x => x.TemplateCode);
            foreach (var message in messages)
            {
                var template = codeTemplate[message.TemplateCode];
                result[(int)template.Priority].Add(( template, message ));
            }

            return new PriorityMessageGroups(result);
        }
    }
}