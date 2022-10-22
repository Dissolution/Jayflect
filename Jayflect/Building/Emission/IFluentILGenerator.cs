namespace Jayflect.Building.Emission;

public interface IFluentILGenerator<TGenerator> : IFluentIL<TGenerator>
    where TGenerator : IFluentILGenerator<TGenerator>
{
    /// <summary>
    /// Begins an exception block for a non-filtered exception.
    /// </summary>
    /// <param name="label">
    /// The <see cref="Label"/> for the end of the block.
    /// This will leave you in the correct place to execute <see langword="finally"/> blocks or to finish the <see langword="try"/>.
    /// </param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.beginexceptionblock"/>
    TGenerator BeginExceptionBlock(out Label label, [CallerArgumentExpression(nameof(label))] string lblName = "");

    /// <summary>
    /// Begins a <see langword="catch"/> block.
    /// </summary>
    /// <param name="exceptionType">The <see cref="Type"/> of <see cref="Exception"/>s to catch.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="exceptionType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">If <paramref name="exceptionType"/> is not an <see cref="Exception"/> type.</exception>
    /// <exception cref="ArgumentException">The catch block is within a filtered exception.</exception>
    /// <exception cref="NotSupportedException">The stream being emitted is not currently in an exception block.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.begincatchblock"/>
    TGenerator BeginCatchBlock(Type exceptionType);

    /// <summary>
    /// Begins a <see langword="catch"/> block.
    /// </summary>
    /// <typeparam name="TException">The <see cref="Type"/> of <see cref="Exception"/>s to catch.</typeparam>
    /// <exception cref="ArgumentException">The catch block is within a filtered exception.</exception>
    /// <exception cref="NotSupportedException">The stream being emitted is not currently in an exception block.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.begincatchblock"/>
    TGenerator BeginCatchBlock<TException>() where TException : Exception
        => BeginCatchBlock(typeof(TException));

    /// <summary>
    /// Begins a <see langword="finally"/> block in the stream.
    /// </summary>
    /// <exception cref="NotSupportedException">The stream being emitted is not currently in an exception block.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.beginfinallyblock"/>
    TGenerator BeginFinallyBlock();

    /// <summary>
    /// Begins an exception block for a filtered exception.
    /// </summary>
    /// <exception cref="NotSupportedException">The stream being emitted is not currently in an exception block.</exception>
    /// <exception cref="NotSupportedException">This <see cref="Jay.Reflection.Building.Emission.IILGenerator{TGenerator}"/> belongs to a <see cref="DynamicMethod"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.beginexceptfilterblock"/>
    TGenerator BeginExceptFilterBlock();

    /// <summary>
    /// Begins an exception fault block in the stream.
    /// </summary>
    /// <exception cref="NotSupportedException">The stream being emitted is not currently in an exception block.</exception>
    /// <exception cref="NotSupportedException">This <see cref="Jay.Reflection.Building.Emission.IILGenerator{TGenerator}"/> belongs to a <see cref="DynamicMethod"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.beginfaultblock"/>
    TGenerator BeginFaultBlock();

    /// <summary>
    /// Ends an exception block.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this operation occurs in an unexpected place in the stream.</exception>
    /// <exception cref="NotSupportedException">If the stream being emitted is not currently in an exception block.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.endexceptionblock"/>
    TGenerator EndExceptionBlock();

     #region Scope
    /// <summary>
    /// Begins a lexical scope.
    /// </summary>
    /// <exception cref="NotSupportedException">This <see cref="IILGenerator{T}"/> belongs to a <see cref="DynamicMethod"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.beginscope?view=netcore-3.0"/>
    TGenerator BeginScope();

    /// <summary>
    /// Ends a lexical scope.
    /// </summary>
    /// <exception cref="NotSupportedException">If this <see cref="IILGenerator{T}"/> belongs to a <see cref="DynamicMethod"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.endscope?view=netcore-3.0"/>
    TGenerator EndScope();

    /// <summary>
    /// Specifies the <see langword="namespace"/> to be used in evaluating locals and watches for the current active lexical scope.
    /// </summary>
    /// <param name="usingNamespace">The namespace to be used in evaluating locals and watches for the current active lexical scope.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="usingNamespace"/> is <see langword="null"/> or has a Length of 0.</exception>
    /// <exception cref="NotSupportedException">If this <see cref="IILGenerator{T}"/> belongs to a <see cref="DynamicMethod"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.usingnamespace?view=netcore-3.0"/>
    TGenerator UsingNamespace(string usingNamespace);

    TGenerator Scoped(Action<TGenerator> scopedBlock)
    {
        BeginScope();
        scopedBlock((TGenerator)this);
        EndScope();
        return (TGenerator)this;
    }
    #endregion

    /// <summary>
    /// Declares a <see cref="LocalBuilder"/> variable of the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="localType">The type of the <see cref="LocalBuilder"/>.</param>
    /// <param name="local">Returns the declared <see cref="LocalBuilder"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="localType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">If <paramref name="localType"/> was created with <see cref="TypeBuilder.CreateType"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.declarelocal#System_Reflection_Emit_ILGenerator_DeclareLocal_System_Type_"/>
    TGenerator DeclareLocal(Type localType, out LocalBuilder local, [CallerArgumentExpression("local")] string localName = "");

    /// <summary>
    /// Declares a <see cref="LocalBuilder"/> variable of the specified <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="LocalBuilder"/>.</typeparam>
    /// <param name="local">Returns the declared <see cref="LocalBuilder"/>.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.declarelocal#System_Reflection_Emit_ILGenerator_DeclareLocal_System_Type_"/>
    TGenerator DeclareLocal<T>(out LocalBuilder local, [CallerArgumentExpression("local")] string localName = "")
        => DeclareLocal(typeof(T), out local, localName);

    /// <summary>
    /// Declares a <see cref="LocalBuilder"/> variable of the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="localType">The type of the <see cref="LocalBuilder"/>.</param>
    /// <param name="pinned">Whether or not the <see cref="LocalBuilder"/> should be pinned in memory.</param>
    /// <param name="local">Returns the declared <see cref="LocalBuilder"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="localType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">If <paramref name="localType"/> was created with <see cref="TypeBuilder.CreateType"/>.</exception>
    /// <exception cref="InvalidOperationException">If the method body of the enclosing method was created with <see cref="M:MethodBuilder.CreateMethodBody"/>.</exception>
    /// <exception cref="NotSupportedException">If the method this <see cref="Jay.Reflection.Building.Emission.IILGenerator{TGenerator}"/> is associated with is not wrapping a <see cref="MethodBuilder"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.declarelocal#System_Reflection_Emit_ILGenerator_DeclareLocal_System_Type_System_Boolean_"/>
    TGenerator DeclareLocal(Type localType, bool pinned, out LocalBuilder local, [CallerArgumentExpression("local")] string localName = "");

    /// <summary>
    /// Declares a <see cref="LocalBuilder"/> variable of the specified <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="LocalBuilder"/>.</typeparam>
    /// <param name="pinned">Whether or not the <see cref="LocalBuilder"/> should be pinned in memory.</param>
    /// <param name="local">Returns the declared <see cref="LocalBuilder"/>.</param>
    /// <exception cref="InvalidOperationException">If the method body of the enclosing method was created with <see cref="M:MethodBuilder.CreateMethodBody"/>.</exception>
    /// <exception cref="NotSupportedException">If the method this <see cref="Jay.Reflection.Building.Emission.IILGenerator{TGenerator}"/> is associated with is not wrapping a <see cref="MethodBuilder"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.declarelocal#System_Reflection_Emit_ILGenerator_DeclareLocal_System_Type_System_Boolean_"/>
    TGenerator DeclareLocal<T>(bool pinned, out LocalBuilder local, [CallerArgumentExpression("local")] string localName = "")
        => DeclareLocal(typeof(T), pinned, out local, localName);

    /// <summary>
    /// Declares a new <see cref="Label"/>.
    /// </summary>
    /// <param name="label">Returns the new <see cref="Label"/> that can be used for branching.</param>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.definelabel"/>
    TGenerator DefineLabel(out Label label, [CallerArgumentExpression("label")] string lblName = "");

    /// <summary>
    /// Marks the stream's current position with the given <see cref="Label"/>.
    /// </summary>
    /// <param name="label">The <see cref="Label"/> for which to set an index.</param>
    /// <exception cref="ArgumentException">If the <paramref name="label"/> has an invalid index.</exception>
    /// <exception cref="ArgumentException">If the <paramref name="label"/> has already been marked.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.marklabel"/>
    TGenerator MarkLabel(Label label);

    /// <summary>
    /// Puts a <see cref="OpCodes.Call"/>, <see cref="OpCodes.Callvirt"/>, or <see cref="OpCodes.Newobj"/> instruction onto the stream to call a <see langword="varargs"/> <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="methodInfo">The <see langword="varargs"/> <see cref="MethodInfo"/> to be called.</param>
    /// <param name="optionalParameterTypes">The types of the Option arguments if the method is a <see langword="varargs"/> method; otherwise, <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emitcall"/>
    TGenerator EmitCall(MethodInfo methodInfo, Type[]? optionalParameterTypes);

    /// <summary>
    /// Puts a <see cref="OpCodes.Calli"/> instruction onto the stream, specifying an unmanaged calling convention for the indirect call.
    /// </summary>
    /// <param name="callingConvention">The managed calling conventions to be used.</param>
    /// <param name="returnType">The <see cref="Type"/> of the result.</param>
    /// <param name="parameterTypes">The types of the required arguments to the instruction.</param>
    /// <param name="optionalParameterTypes">The types of the Option arguments for <see langword="varargs"/> calls.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="returnType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">If <paramref name="optionalParameterTypes"/> is not <see langword="null"/> or empty but <paramref name="callingConvention"/> does not include the <see cref="CallingConventions.VarArgs"/> flag.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emitcalli#System_Reflection_Emit_ILGenerator_EmitCalli_System_Reflection_Emit_OpCode_System_Reflection_CallingConventions_System_Type_System_Type___System_Type___"/>
    TGenerator EmitCalli(CallingConventions callingConvention, Type? returnType, Type[]? parameterTypes, Type[]? optionalParameterTypes);

    /// <summary>
    /// Puts a <see cref="OpCodes.Calli"/> instruction onto the stream, specifying an unmanaged calling convention for the indirect call.
    /// </summary>
    /// <param name="unmanagedCallConv">The unmanaged calling convention to be used.</param>
    /// <param name="returnType">The <see cref="Type"/> of the result.</param>
    /// <param name="parameterTypes">The types of the required arguments to the instruction.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="returnType"/> is <see langword="null"/>.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.ilgenerator.emitcalli#System_Reflection_Emit_ILGenerator_EmitCalli_System_Reflection_Emit_OpCode_System_Runtime_InteropServices_CallingConvention_System_Type_System_Type___"/>
    TGenerator EmitCalli(CallingConvention unmanagedCallConv, Type? returnType, Type[]? parameterTypes);
}