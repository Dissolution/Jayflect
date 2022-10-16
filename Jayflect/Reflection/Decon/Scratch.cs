namespace Jay.Reflection.Decon;

public class Scratch
{
    static Scratch()
    {
        
    }
}

public interface ISerializer
{
    
}

public interface ISerializer<TIn, TOut> : ISerializer
{
    
}

public interface IPart
{
    string? Name { get; }
    Type Type { get; }

    object? GetObjectValue();
}