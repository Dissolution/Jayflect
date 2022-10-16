namespace Jay.Reflection.Exceptions;

public class AdapterException : ReflectionException
{
    public AdapterException(string? message = null, Exception? innerException = null) 
        : base(message, innerException)
    {
    }
}