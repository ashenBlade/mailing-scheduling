using MailingScheduler.Core;
using MailingScheduler.PlanningStrategy;

namespace MailingScheduler.Generator;

public static class MessagesGenerator
{
    public static IEnumerable<Message> GenerateMessages(DateTime currentTime, TemplateInfo[] templateInfos)
    {
        var generator = new MessageDataGenerator(currentTime);
        foreach (var info in templateInfos)
        {
            var totalMessages = generator.GetMessagesCount();
            for (int i = 0; i < totalMessages; i++)
            {
                var (offset, start, end) = generator.GenerateMessageData();
                yield return new Message(Guid.NewGuid(), info.TemplateCode, start, end, offset);
            }
        }
    }

    private class MessageDataGenerator
    {
        private readonly DateTime _currentTime;
        private readonly Random _random = new();

        public MessageDataGenerator(DateTime currentTime)
        {
            _currentTime = currentTime;
        }

        public int GetMessagesCount()
        {
            return _random.Next(100, 10000);
        }
        
        public (int Offset, DateTime StartTime, DateTime EndTime) GenerateMessageData()
        {
            var offset = _random.Next(-3, 10);
            var startTime = _currentTime - TimeSpan.FromHours(_random.Next(0, 12));
            var endTime = _currentTime + TimeSpan.FromHours(_random.Next(0, 12));
            return ( offset, startTime, endTime );
        }
    }
}