using Jay.Dumping;

namespace Jayflect.Exceptions;

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

public class RuntimeBuildException : Exception
{
    public RuntimeBuildException(
        ref DumpStringHandler message, 
        Exception? innerException = null)
        : base(message.ToStringAndClear(), innerException)
    {

    }
    
    public RuntimeBuildException(
        string? message = null, 
        Exception? innerException = null)
        : base(message, innerException)
    {

    }
}