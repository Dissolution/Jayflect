using Jay.Dumping;
using Jayflect.Exceptions;

namespace Jayflect.Building.Emission;

public class AdapterException : RuntimeBuildException
{
    private static string CreateMessage(MethodBase method,
        DelegateSignature delegateSig,
        ref DumpStringHandler message)
    {
        var stringHandler = new DumpStringHandler();
        stringHandler.Write("Could not adapt '");
        stringHandler.Dump(delegateSig);
        stringHandler.Write(" to call ");
        stringHandler.Dump(method);
        string msg = message.ToStringAndClear();
        if (msg.Length > 0)
        {
            stringHandler.Write(": ");
            stringHandler.Write(msg);
        }
        return stringHandler.ToStringAndClear();
    }

    public AdapterException(MethodBase method, DelegateSignature delegateSig,
        ref DumpStringHandler message,
        Exception? innerException = null)
        : base(CreateMessage(method, delegateSig, ref message), innerException)
    {

    }
    
    public AdapterException(ref DumpStringHandler message,
        Exception? innerException = null)
        : base(ref message, innerException)
    {

    }
}