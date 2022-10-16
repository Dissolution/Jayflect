/*using System.Reflection;
using System.Reflection.Emit;
using Jay.Reflection.Building.Emission;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace Jay.Reflection.Scratch;

public interface IOpEmitter<TEmitter>
    where TEmitter : IOpEmitter<TEmitter>
{
    #region Emit
    TEmitter Emit(OpCode opCode);

    TEmitter Emit(OpCode opCode, sbyte int8);

    TEmitter Emit(OpCode opCode, int in32);

    TEmitter Emit(OpCode opCode, long int64);

    TEmitter Emit(OpCode opCode, float f32);

    TEmitter Emit(OpCode opCode, double f64);

    TEmitter Emit(OpCode opCode, MethodBase method);
    #endregion

    #region Arguments
    #region Load
    /// <summary>
    /// <c>ldarg.0</c>: Loads the argument at index 0 onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_0"/>
    TEmitter Ldarg_0() => Emit(OpCodes.Ldarg_0);

    /// <summary>
    /// <c>ldarg.1</c> - Loads the argument at index 1 onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_1"/>
    TEmitter Ldarg_1() => Emit(OpCodes.Ldarg_1);

    /// <summary>
    /// <c>ldarg.2</c> - Loads the argument at index 2 onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_2"/>
    TEmitter Ldarg_2() => Emit(OpCodes.Ldarg_2);

    /// <summary>
    /// <c>ldarg.3</c> - Loads the argument at index 3 onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_3"/>
    TEmitter Ldarg_3() => Emit(OpCodes.Ldarg_3);

    /// <summary>
    /// Loads the argument with the specified <paramref name="index"/> onto the stack.
    /// </summary>
    /// <param name="index">The index of the argument to load.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is invalid.</exception>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg"/>
    public TEmitter Ldarg(int index)
    {
        if (index < 0 || index > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Argument index must be between 0 and {short.MaxValue}");
        if (index == 0)
            return Emit(OpCodes.Ldarg_0);
        if (index == 1)
            return Emit(OpCodes.Ldarg_1);
        if (index == 2)
            return Emit(OpCodes.Ldarg_2);
        if (index == 3)
            return Emit(OpCodes.Ldarg_3);
        if (index <= byte.MaxValue)
            return Emit(OpCodes.Ldarg_S, (byte)index);
        return Emit(OpCodes.Ldarg, (short)index);
    }

    /// <summary>
    /// <c>ldarg.s</c> - Loads the argument (referenced by a specified short form index) onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <param name="index">The argument index.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarg_s"/>
    TEmitter Ldarg_S(byte index) => Ldarg(index);


    /// <summary>
    /// Loads the address of the argument with the specified <paramref name="index"/> onto the stack.
    /// </summary>
    /// <param name="index">The index of the argument address to load.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is invalid.</exception>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarga"/>
    TEmitter Ldarga(int index)
    {
        if (index < 0 || index > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Argument index must be between 0 and {short.MaxValue}");
        if (index <= byte.MaxValue)
            return Emit(OpCodes.Ldarga_S, (byte)index);
        return Emit(OpCodes.Ldarga, (short)index);
    }

    /// <summary>
    /// <c>ldarga.s</c> - Load an argument address, in short form, onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <param name="index">The argument index.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldarga_s"/>
    TEmitter Ldarga_S(byte index) => Ldarga(index);
    #endregion
    #region Store
    /// <summary>
    /// Stores the value on top of the stack in the argument at the given <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index of the argument.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.starg"/>
    TEmitter Starg(int index)
    {
        if (index < 0 || index > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Argument index must be between 0 and {short.MaxValue}");
        if (index <= byte.MaxValue)
            return Emit(OpCodes.Starg_S, (byte)index);
        return Emit(OpCodes.Starg, (short)index);
    }

    /// <summary>
    /// <c>starg.s</c> - Stores the value on top of the evaluation stack in the argument slot at a specified index, short form.
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <param name="index">The argument index.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.starg_s"/>
    TEmitter Starg_S(byte index) => Starg(index);

    #endregion
    #endregion

    #region Debugging
    /// <summary>
    /// <c>nop</c>: Fills space if opCodes are patched. No meaningful operation is performed, although a processing cycle can be consumed.
    /// <para>Stack Transition: none</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.nop"/>
    TEmitter Nop() => Emit(OpCodes.Nop);

    /// <summary>
    /// <c>break</c>: Signals the Common Language Infrastructure (CLI) to inform the debugger that a break point has been tripped.
    /// <para>Stack Transition: none</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.break"/>
    TEmitter Break() => Emit(OpCodes.Break);
    #endregion

    #region Execution Flow
    /// <summary>
    /// <c>ret</c> - Returns from the current method, pushing a return value (if present) from the callee's evaluation stack onto the caller's evaluation stack.
    /// <para>Stack Transition: ..., return value (if method does not return <c>void</c>) -&gt; ...</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ret"/>
    TEmitter Ret() => Emit(OpCodes.Ret);

    #region Branching
    /// <summary>
    /// <c>br.s</c> - Unconditionally transfers control to a target instruction (short form).
    /// <para>Stack Transition: none</para>
    /// </summary>
    /// <param name="label">The target label.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="label"/> does not qualify for short-form instructions.</exception>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.br_s?view=netcore-3.0"/>
    TEmitter Br_S(Label label) => Br(label);

    #endregion
    #endregion

    #region Locals
    #region Load
    /// <summary>
    /// <c>ldloc.0</c> - Loads the local variable at index 0 onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_0"/>
    TEmitter Ldloc_0() => Emit(OpCodes.Ldloc_0);

    /// <summary>
    /// <c>ldloc.1</c> - Loads the local variable at index 1 onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_1"/>
    TEmitter Ldloc_1() => Emit(OpCodes.Ldloc_1);

    /// <summary>
    /// <c>ldloc.2</c> - Loads the local variable at index 2 onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_2"/>
    TEmitter Ldloc_2() => Emit(OpCodes.Ldloc_2);

    /// <summary>
    /// <c>ldloc.3</c> - Loads the local variable at index 3 onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_3"/>
    TEmitter Ldloc_3() => Emit(OpCodes.Ldloc_3);

    /// <summary>
    /// <c>ldloc.s</c> - Loads the local variable at a specific index onto the evaluation stack, short form.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <param name="index">The local variable index.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_s"/>
    TEmitter Ldloc_S(byte index) => Ldloc(index);

    /// <summary>
    /// <c>ldloca.s</c> - Loads the address of the local variable at a specific index onto the evaluation stack, short form.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <param name="index">The local variable index.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldloc_s"/>
    TEmitter Ldloca_S(byte index) => Ldloca(index);

    #endregion

    #region Store
    /// <summary>
    /// <c>stloc.0</c> - Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 0.
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_0"/>
    TEmitter Stloc_0() => Emit(OpCodes.Stloc_0);

    /// <summary>
    /// <c>stloc.1</c> - Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 1.
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_1"/>
    TEmitter Stloc_1() => Emit(OpCodes.Stloc_1);

    /// <summary>
    /// <c>stloc.2</c> - Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 2.
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_2"/>
    TEmitter Stloc_2() => Emit(OpCodes.Stloc_2);

    /// <summary>
    /// <c>stloc.3</c> - Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 3.
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_3"/>
    TEmitter Stloc_3() => Emit(OpCodes.Stloc_3);

    /// <summary>
    /// <c>stloc.s</c> - Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index (short form).
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <param name="index">The local variable index.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.stloc_s"/>
    TEmitter Stloc_S(byte index) => Stloc(index);

    #endregion

    #endregion

    #region Load Value
    /// <summary>
    /// <c>ldnull</c> - Pushes a null reference (type O) onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., O</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldnull"/>
    TEmitter Ldnull() => Emit(OpCodes.Ldnull);

    #region Load Constant
    #region Load Int32
    /// <summary>
    /// <c>ldc.i4.m1</c> - Pushes the integer value of -1 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_m1"/>
    TEmitter Ldc_I4_M1() => Emit(OpCodes.Ldc_I4_M1);

    /// <summary>
    /// <c>ldc.i4.0</c> - Pushes the integer value of 0 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_0"/>
    TEmitter Ldc_I4_0() => Emit(OpCodes.Ldc_I4_0);

    /// <summary>
    /// <c>ldc.i4.1</c> - Pushes the integer value of 1 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_1"/>
    TEmitter Ldc_I4_1() => Emit(OpCodes.Ldc_I4_1);

    /// <summary>
    /// <c>ldc.i4.2</c> - Pushes the integer value of 2 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_2"/>
    TEmitter Ldc_I4_2() => Emit(OpCodes.Ldc_I4_2);

    /// <summary>
    /// <c>ldc.i4.3</c> - Pushes the integer value of 3 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_3"/>
    TEmitter Ldc_I4_3() => Emit(OpCodes.Ldc_I4_3);

    /// <summary>
    /// <c>ldc.i4.4</c> - Pushes the integer value of 4 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_4"/>
    TEmitter Ldc_I4_4() => Emit(OpCodes.Ldc_I4_4);

    /// <summary>
    /// <c>ldc.i4.5</c> - Pushes the integer value of 5 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_5"/>
    TEmitter Ldc_I4_5() => Emit(OpCodes.Ldc_I4_5);

    /// <summary>
    /// <c>ldc.i4.6</c> - Pushes the integer value of 6 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_6"/>
    TEmitter Ldc_I4_6() => Emit(OpCodes.Ldc_I4_6);

    /// <summary>
    /// <c>ldc.i4.7</c> - Pushes the integer value of 7 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_7"/>
    TEmitter Ldc_I4_7() => Emit(OpCodes.Ldc_I4_7);

    /// <summary>
    /// <c>ldc.i4.8</c> - Pushes the integer value of 8 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_8"/>
    TEmitter Ldc_I4_8() => Emit(OpCodes.Ldc_I4_8);

    /// <summary>
    /// <c>ldc.i4.s</c> - Pushes the supplied int8 value onto the evaluation stack as an int32, short form.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <param name="value">The int8 value.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4_s"/>
    TEmitter Ldc_I4_S(sbyte value) => Ldc_I4(value);

    /// <summary>
    /// <c>ldc.i4</c> - Pushes a supplied value of type int32 onto the evaluation stack as an int32.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i4"/>
    TEmitter Ldc_I4(int value)
    {
        return value switch
        {
            -1 => Emit(OpCodes.Ldc_I4_M1),
            0 => Emit(OpCodes.Ldc_I4_0),
            1 => Emit(OpCodes.Ldc_I4_1),
            2 => Emit(OpCodes.Ldc_I4_2),
            3 => Emit(OpCodes.Ldc_I4_3),
            4 => Emit(OpCodes.Ldc_I4_4),
            5 => Emit(OpCodes.Ldc_I4_5),
            6 => Emit(OpCodes.Ldc_I4_6),
            7 => Emit(OpCodes.Ldc_I4_7),
            8 => Emit(OpCodes.Ldc_I4_8),
            >= sbyte.MinValue and <= sbyte.MaxValue => Emit(OpCodes.Ldc_I4_S, (sbyte)value),
            _ => Emit(OpCodes.Ldc_I4, value)
        };
    }
    #endregion

    /// <summary>
    /// <c>ldc.i8</c> - Pushes a supplied value of type int64 onto the evaluation stack as an int64.
    /// <para>Stack Transition: ... -&gt; ..., I8</para>
    /// </summary>
    /// <param name="value">The <see cref="long"/> value.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_i8"/>
    TEmitter Ldc_I8(long value) => Emit(OpCodes.Ldc_I8, value);

    /// <summary>
    /// <c>ldc.r4</c> - Pushes a supplied value of type float32 onto the evaluation stack as type F (float).
    /// <para>Stack Transition: ... -&gt; ..., R4</para>
    /// </summary>
    /// <param name="value">The operand.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_r4"/>
    TEmitter Ldc_R4(float value) => Emit(OpCodes.Ldc_R4, value);

    /// <summary>
    /// <c>ldc.r8</c> - Pushes a supplied value of type float64 onto the evaluation stack as type F (float).
    /// <para>Stack Transition: ... -&gt; ..., R8</para>
    /// </summary>
    /// <param name="value">The operand.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.ldc_r8"/>
    TEmitter Ldc_R8(double value) => Emit(OpCodes.Ldc_R8, value);



    #endregion
    #endregion;

    #region Method Related
    /// <summary>
    /// <c>call</c> - Calls the method indicated by the passed method descriptor.
    /// <para>Stack Transition: ..., arg0, arg1, ..., argN -&gt; ..., return value (if callee does not return <c>void</c>)</para>
    /// </summary>
    /// <param name="method">The method reference.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.call"/>
    /// <seealso href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.callvirt"/>
    TEmitter Call(MethodInfo method) => Emit(method.GetCallOpCode(), method);

    /// <summary>
    /// <c>jmp</c> - Exits current method and jumps to specified method.
    /// <para>Stack Transition: none</para>
    /// </summary>
    /// <param name="method">The method reference.</param>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.jmp"/>
    TEmitter Jmp(MethodInfo method)
    {
        ArgumentNullException.ThrowIfNull(method);
        return Emit(OpCodes.Jmp, method);
    }
    #endregion

    #region Stack Manipulation
    /// <summary>
    /// <c>dup</c> - Copies the current topmost value on the evaluation stack, and then pushes the copy onto the evaluation stack.
    /// <para>Stack Transition: ..., value -&gt; ..., value, value</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.dup"/>
    TEmitter Dup() => Emit(OpCodes.Dup);

    /// <summary>
    /// <c>pop</c> - Removes the value currently on top of the evaluation stack.
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <see href="http://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.pop"/>
    TEmitter Pop() => Emit(OpCodes.Pop);
    #endregion
}*/