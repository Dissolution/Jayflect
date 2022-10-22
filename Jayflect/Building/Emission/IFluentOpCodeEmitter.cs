namespace Jayflect.Building.Emission;

public interface IFluentOpCodeEmitter<TEmitter> : IFluentIL<TEmitter>
    where TEmitter : IFluentOpCodeEmitter<TEmitter>
{
    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_"/>
    TEmitter Emit(OpCode opCode);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="byte"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="arg">The numeric value to emit.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Byte_"/>
    TEmitter Emit(OpCode opCode, byte arg);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="sbyte"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="arg">The numeric value to emit.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_SByte_"/>
    TEmitter Emit(OpCode opCode, sbyte arg);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="short"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="arg">The numeric value to emit.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Int16_"/>
    TEmitter Emit(OpCode opCode, short arg);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="int"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="arg">The numeric value to emit.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Int32_"/>
    TEmitter Emit(OpCode opCode, int arg);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="long"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="arg">The numeric value to emit.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Int64_"/>
    TEmitter Emit(OpCode opCode, long arg);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="float"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="arg">The numeric value to emit.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Single_"/>
    TEmitter Emit(OpCode opCode, float arg);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="double"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="arg">The numeric value to emit.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Double_"/>
    TEmitter Emit(OpCode opCode, double arg);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the metadata token for the given <see cref="string"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="str">The <see cref="string"/>to emit.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_String_"/>
    TEmitter Emit(OpCode opCode, string? str);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream and leaves space to include a <see cref="Label"/> when fixes are done.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="label">The <see cref="Label"/> to branch from this location.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Reflection_Emit_Label_"/>
    TEmitter Emit(OpCode opCode, Label label);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream and leaves space to include a <see cref="Label"/> when fixes are done.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="labels">The <see cref="Label"/>s of which to branch to from this locations.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="labels"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="labels"/> is empty.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Reflection_Emit_Label___"/>
    TEmitter Emit(OpCode opCode, params Label[] labels);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the index of the given <see cref="LocalBuilder"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="local">The <see cref="LocalBuilder"/> to emit the index of.</param>
    /// <exception cref="InvalidOperationException">If <paramref name="opCode"/> is a single-byte instruction and <paramref name="local"/> has an index greater than <see cref="byte.MaxValue"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Reflection_Emit_LocalBuilder_"/>
    TEmitter Emit(OpCode opCode, LocalBuilder local);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="FieldInfo"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="field">The <see cref="ArgumentNullException"/> to emit.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="field"/> is <see langword="null"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Reflection_FieldInfo_"/>
    TEmitter Emit(OpCode opCode, FieldInfo field);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the metadata token for the given <see cref="ConstructorInfo"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="ctor">The <see cref="ConstructorInfo"/> to emit.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="ctor"/> is <see langword="null"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Reflection_ConstructorInfo_"/>
    TEmitter Emit(OpCode opCode, ConstructorInfo ctor);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the metadata token for the given <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="method">The <see cref="MethodInfo"/> to emit.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="method"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">If <paramref name="method"/> is a generic method for which <see cref="MethodBase.IsGenericMethodDefinition"/> is <see langword="false"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Reflection_MethodInfo_"/>
    TEmitter Emit(OpCode opCode, MethodInfo method);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the metadata token for the given <see cref="Type"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="type">The <see cref="Type"/> to emit.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Type_"/>
    TEmitter Emit(OpCode opCode, Type type);

    /// <summary>
    /// Emits an <see cref="OpCode"/> onto the stream followed by the given <see cref="SignatureHelper"/>.
    /// </summary>
    /// <param name="opCode">The MSIL instruction to be emitted onto the stream.</param>
    /// <param name="signature">A helper for constructing a signature token.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="signature"/> is <see langword="null"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emit#System_Reflection_Emit_ILGenerator_Emit_System_Reflection_Emit_OpCode_System_Reflection_Emit_SignatureHelper_"/>
    TEmitter Emit(OpCode opCode, SignatureHelper signature);
}