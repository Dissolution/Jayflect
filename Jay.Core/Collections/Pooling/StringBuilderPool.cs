using System.Text;

namespace Jay.Collections.Pooling;

public sealed class StringBuilderPool : ObjectPool<StringBuilder>
{
    public static StringBuilderPool Shared { get; } = new();
    
    public StringBuilderPool()
        : base(factory: () => new StringBuilder(), clean: builder => builder.Clear())
    { }

    public string ReturnToString(StringBuilder builder)
    {
        string str = builder.ToString();
        this.Return(builder);
        return str;
    }
}