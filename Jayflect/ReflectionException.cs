using Jay.Dumping;

namespace Jayflect;

/// <summary>
/// An <see cref="Exception"/> thrown during reflection
/// </summary>
public class ReflectionException : Exception
{
    public ReflectionException(
        ref DumpStringHandler message, 
        Exception? innerException = null)
        : base(message.ToStringAndClear(), innerException)
    {

    }
}