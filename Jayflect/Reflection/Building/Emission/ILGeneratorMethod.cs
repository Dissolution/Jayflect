namespace Jay.Reflection.Building.Emission;

public enum ILGeneratorMethod
{
    None = 0,

    BeginCatchBlock,            // arg = Type
    BeginExceptFilterBlock,     // arg = void
    BeginExceptionBlock,        // arg = Label
    EndExceptionBlock,          // arg = void
    BeginFaultBlock,            // arg = void
    BeginFinallyBlock,          // arg = void
    BeginScope,                 // arg = void
    EndScope,                   // arg = void
    UsingNamespace,             // arg = string
    DeclareLocal,               // arg = (Type, LocalBuilder) / (Type, bool, LocalBuilder)
    DefineLabel,                // arg = Label
    MarkLabel,                  // arg = Label
    EmitCall,                   // arg = (MethodInfo, Type[])
    EmitCalli,                  // arg = (CallingConvention, Type, Type[]) / (CallingConventions, Type, Type[], Type[])
    WriteLine,                  // arg = string, FieldInfo, LocalBuilder
    ThrowException,             // arg = Type
}