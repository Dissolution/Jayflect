namespace Jayflect;

/// <summary>
/// An <see cref="Exception"/> thrown during reflection
/// </summary>
public class ReflectionException : Exception
{
    public ReflectionException(string? message = null, Exception? innerException = null)
        : base(message, innerException)
    {

    }
}