/*using InlineIL;

namespace Jay.Reflection.Scratch;

public interface IFluentOpEmitter<TEmitter> : IOpEmitter<TEmitter>
    where TEmitter : IFluentOpEmitter<TEmitter>
{
    #region Arguments
    #region Load
    /// <summary>
    /// <c>ldarg.s</c> - Loads the argument (referenced by a specified short form index) onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <param name="argName">The parameter name.</param>
    TEmitter Ldarg_S(string argName);

    /// <summary>
    /// <c>ldarga.s</c> - Load an argument address, in short form, onto the evaluation stack.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <param name="argName">The parameter name.</param>
    TEmitter Ldarga_S(string argName);
    #endregion
    #region Store

    /// <summary>
    /// <c>starg.s</c> - Stores the value on top of the evaluation stack in the argument slot at a specified index, short form.
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <param name="argName">The parameter name.</param>
    TEmitter Starg_S(string argName);
    #endregion
    #endregion

    #region Execution Flow
    #region Branching
    /// <summary>
    /// <c>br.s</c> - Unconditionally transfers control to a target instruction (short form).
    /// <para>Stack Transition: none</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Br_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>brfalse.s</c> - Transfers control to a target instruction if value is false, a null reference, or zero.
    /// <para>Stack Transition: ..., I -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Brfalse_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>brtrue.s</c> - Transfers control to a target instruction (short form) if value is true, not null, or non-zero.
    /// <para>Stack Transition: ..., I -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Brtrue_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>beq.s</c> - Transfers control to a target instruction (short form) if two values are equal.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Beq_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bge.s</c> - Transfers control to a target instruction (short form) if the first value is greater than or equal to the second value.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bge_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bgt.s</c> - Transfers control to a target instruction (short form) if the first value is greater than the second value.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bgt_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>ble.s</c> - Transfers control to a target instruction (short form) if the first value is less than or equal to the second value.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Ble_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>blt.s</c> - Transfers control to a target instruction (short form) if the first value is less than the second value.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Blt_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bne.un.s</c> - Transfers control to a target instruction (short form) when two unsigned integer values or unordered float values are not equal.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bne_Un_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bge.un.s</c> - Transfers control to a target instruction (short form) if the first value is greater than the second value, when comparing unsigned integer values or unordered float values.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bge_Un_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bgt.un.s</c> - Transfers control to a target instruction (short form) if the first value is greater than the second value, when comparing unsigned integer values or unordered float values.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bgt_Un_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>ble.un.s</c> - Transfers control to a target instruction (short form) if the first value is less than or equal to the second value, when comparing unsigned integer values or unordered float values.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Ble_Un_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>blt.un.s</c> - Transfers control to a target instruction (short form) if the first value is less than the second value, when comparing unsigned integer values or unordered float values.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Blt_Un_S(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>br</c> - Unconditionally transfers control to a target instruction.
    /// <para>Stack Transition: none</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Br(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>brfalse</c> - Transfers control to a target instruction if value is false, a null reference (Nothing in Visual Basic), or zero.
    /// <para>Stack Transition: ..., I -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Brfalse(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>brtrue</c> - Transfers control to a target instruction if value is true, not null, or non-zero.
    /// <para>Stack Transition: ..., I -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Brtrue(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>beq</c> - Transfers control to a target instruction if two values are equal.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Beq(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bge</c> - Transfers control to a target instruction if the first value is greater than or equal to the second value.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bge(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bgt</c> - Transfers control to a target instruction if the first value is greater than the second value.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bgt(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>ble</c> - Transfers control to a target instruction if the first value is less than or equal to the second value.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Ble(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>blt</c> - Transfers control to a target instruction if the first value is less than the second value.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Blt(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bne.un</c> - Transfers control to a target instruction when two unsigned integer values or unordered float values are not equal.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bne_Un(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bge.un</c> - Transfers control to a target instruction if the first value is greater than the second value, when comparing unsigned integer values or unordered float values.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bge_Un(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>bgt.un</c> - Transfers control to a target instruction if the first value is greater than the second value, when comparing unsigned integer values or unordered float values.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Bgt_Un(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>ble.un</c> - Transfers control to a target instruction if the first value is less than or equal to the second value, when comparing unsigned integer values or unordered float values.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Ble_Un(string labelName)
        => IL.Throw();

    /// <summary>
    /// <c>blt.un</c> - Transfers control to a target instruction if the first value is less than the second value, when comparing unsigned integer values or unordered float values.
    /// <para>Stack Transition: ..., value, value -&gt; ...</para>
    /// </summary>
    /// <param name="labelName">The target label name.</param>
    public static void Blt_Un(string labelName)
        => IL.Throw();
    #endregion

    #region Locals
    #region Load

    /// <summary>
    /// <c>ldloc.s</c> - Loads the local variable at a specific index onto the evaluation stack, short form.
    /// <para>Stack Transition: ... -&gt; ..., value</para>
    /// </summary>
    /// <param name="localName">The local variable name, declared with <see cref="IL.DeclareLocals(LocalVar[])" />.</param>
    TEmitter Ldloc_S(string localName);

    /// <summary>
    /// <c>ldloca.s</c> - Loads the address of the local variable at a specific index onto the evaluation stack, short form.
    /// <para>Stack Transition: ... -&gt; ..., I</para>
    /// </summary>
    /// <param name="localName">The local variable name, declared with <see cref="IL.DeclareLocals(LocalVar[])" />.</param>
    TEmitter Ldloca_S(string localName);
    #endregion
    #region Store

    /// <summary>
    /// <c>stloc.s</c> - Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index (short form).
    /// <para>Stack Transition: ..., value -&gt; ...</para>
    /// </summary>
    /// <param name="localName">The local variable name, declared with <see cref="IL.DeclareLocals(LocalVar[])" />.</param>
    TEmitter Stloc_S(string localName);

    #endregion

    #endregion
}*/