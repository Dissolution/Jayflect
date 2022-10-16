namespace Jay.Reflection.Building.Adapting;

public enum Safety
{
    // Safe
    Safe = 0,

    // Weird, but OK
    AllowRefAsIn,
    AllowRefAsOut,
    AllowReturnDiscard,

    // Side Effects
    AllowNonRefStructInstance,
    AllowReturnDefault,

    // Dangerous
    AllowInAsRef,
    AllowInAsOut,
    AllowOutAsRef,

    // WTF?
    AllowOutAsIn,
    
}