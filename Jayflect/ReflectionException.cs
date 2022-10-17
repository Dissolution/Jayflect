using Jayflect.Formatting;

namespace Jayflect;

/// <summary>
/// An <see cref="Exception"/> thrown during reflection
/// </summary>
public class ReflectionException : Exception
{
    public ReflectionException(
        ref ReflectInterpolatedStringHandler message, 
        Exception? innerException = null)
        : base(message.ToStringAndClear(), innerException)
    {

    }
}