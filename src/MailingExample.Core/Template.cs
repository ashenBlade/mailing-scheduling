namespace MailingExample.Core;

public class Template : IEquatable<Template>
{
    public Template(string templateCode, Priority priority, IPlanningStrategy strategy)
    {
        TemplateCode = templateCode;
        Priority = priority;
        Strategy = strategy;
    }

    public string TemplateCode { get; init; }
    public Priority Priority { get; init; }
    public IPlanningStrategy Strategy { get; init; }

    public bool Equals(Template? other)
    {
        return other?.TemplateCode == TemplateCode;
    }

    public override bool Equals(object? other)
    {
        return other is Template {TemplateCode: var code} && code == TemplateCode;
    }

    public override int GetHashCode()
    {
        return TemplateCode.GetHashCode();
    }

    public static readonly IEqualityComparer<Template> TemplateCodeComparer = new TemplateCodeEqualityComparer();

    private class TemplateCodeEqualityComparer : IEqualityComparer<Template>
    {
        public bool Equals(Template x, Template y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.TemplateCode == y.TemplateCode;
        }

        public int GetHashCode(Template obj)
        {
            return obj.TemplateCode.GetHashCode();
        }
    }
}