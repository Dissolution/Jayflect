using System.Diagnostics;
using Jay;
using Jay.Extensions;
using Jayflect.Building.Emission;
using Jayflect.Exceptions;

namespace Jayflect.Building.Adaption;

public abstract class Arg : IEquatable<Arg>
{
    public static implicit operator Arg(ParameterInfo parameter) => new ParameterArg(parameter);
    public static implicit operator Arg(Type type) => new TypeArg(type);

    public static bool operator ==(Arg left, Arg right) => left.Equals(right);
    public static bool operator !=(Arg left, Arg right) => !left.Equals(right);

    public static Arg FromParameter(ParameterInfo parameter) => new ParameterArg(parameter);
    public static Arg FromType(Type type) => new TypeArg(type);

    public ParameterAccess Access { get; }

    public Type Type { get; }

    public abstract bool IsOnStack { get; }

    protected Arg(ParameterAccess access, Type type)
    {
        this.Access = access;
        this.Type = type;
    }
    public void Deconstruct(out ParameterAccess access, out Type type)
    {
        access = this.Access;
        type = this.Type;
    }
    public void Deconstruct(out Type type)
    {
        type = this.ToType();
    }

    protected abstract void Load(IFluentILEmitter emitter);
    protected abstract void LoadAddress(IFluentILEmitter emitter);

    public Result CanLoadAs(Arg destArg, out int exactness)
    {
        JayflectException GetNotImplementedEx() => new JayflectException
        {
            Message = Dump($"Cannot load {this} as a {destArg}"),
            InnerException = new NotImplementedException("This may be implemented in the future"),
            Data =
            {
                { "Source", this },
                { "Dest", destArg },
            },
        };

        exactness = int.MaxValue;

        // Unimplemented fast check
        if (Type.IsPointer || destArg.Type.IsPointer)
            return GetNotImplementedEx();

        // ? -> void
        if (destArg.Type == typeof(void))
        {
            if (destArg.Access != ParameterAccess.Default)
                return GetNotImplementedEx();

            // Source is also void?
            if (Type == typeof(void))
            {
                exactness = 0;
            }
            else
            {
                // popping a variable isn't great
                exactness = 10;
            }
            return true;
        }

        if (Type == typeof(void))
        {
            // Creating a value isn't great
            exactness = 10;
            return true;
        }

        /* ? -> ?object
         * Needs to be checked early or it will be caught up in the
         * .Implements() check below
         */
        if (destArg.Type == typeof(object))
        {
            // ?T -> object
            if (destArg.Access != ParameterAccess.Default)
                return GetNotImplementedEx();

            // Boxing is fine
            exactness = 5;
            return true;
        }

        /* ?object -> ?
         * Unboxing
         */
        if (this.Type == typeof(object))
        {
            // Unboxing is fine
            exactness = 5;
            return true;
        }

        // ?T -> ?T
        if (this.Type == destArg.Type)
        {
            // Exact is great
            exactness = 0;
            return true;
        }

        /* ?T:U -> ?U
         * Tests for implements, so we can autocast
         * This also takes care of interfaces
         */
        if (this.Type.Implements(destArg.Type))
        {
            // T:U -> U
            if (this.Access != ParameterAccess.Default || destArg.Access != ParameterAccess.Default)
                return GetNotImplementedEx();

            // Pretty exact
            exactness = 2;
            return true;
        }

        // We don't know how to do this (yet)
        return GetNotImplementedEx();

    }

    public virtual Result TryLoadAs(
        IFluentILEmitter emitter,
        Arg destArg,
        bool emitTypeChecks = false)
    {
        JayflectException GetNotImplementedEx() => new JayflectException
        {
            Message = Dump($"Cannot load {this} as a {destArg}"),
            InnerException = new NotImplementedException("This may be implemented in the future"),
            Data =
            {
                { "Source", this },
                { "Dest", destArg },
            },
        };

        // Unimplemented fast check
        if (Type.IsPointer || destArg.Type.IsPointer)
            return GetNotImplementedEx();

        // ? -> void
        if (destArg.Type == typeof(void))
        {
            if (destArg.Access != ParameterAccess.Default)
                return GetNotImplementedEx();

            // Anything on the stack we have to pop?
            if (IsOnStack)
            {
                emitter.Pop();
            }

            // Done
            return true;
        }

        /* void -> ?
         * Note: this might seem odd (creating a value from nothing),
         * but this is necessary for generic method invocation,
         * where we might adapt an `object? Invoke(params object?[] args)`
         * delegate to a `void Thing(?)` method.
         * In this case, we're just going to return `default(TReturn)`
         */
        if (Type == typeof(void))
        {
            if (destArg.Access == ParameterAccess.Default)
            {
                emitter.LoadDefault(destArg.Type);
            }
            else
            {
                emitter.LoadDefaultAddress(destArg.Type);
            }

            // done
            return true;
        }

        /* ? -> ?object
         * Needs to be checked early or it will be caught up in the
         * .Implements() check below
         */
        if (destArg.Type == typeof(object))
        {
            // ?T -> object
            if (destArg.Access != ParameterAccess.Default)
                return GetNotImplementedEx();

            // We need to get a boxed value

            // Ensure value is on stack
            Load(emitter);

            // ref T -> object
            if (this.Access != ParameterAccess.Default)
            {
                // get the T
                emitter.Ldind(this.Type);
            }

            // If we're not already typeof(object), box us
            if (this.Type != typeof(object))
            {
                emitter.Box(this.Type);
            }

            return true;
        }

        /* ?object -> ?
         * Unboxing
         */
        if (this.Type == typeof(object))
        {
            // We need to unbox a value

            if (emitTypeChecks)
            {
                return GetNotImplementedEx();
            }

            // Ensure value is on stack
            Load(emitter);

            // ref object -> object
            if (this.Access != ParameterAccess.Default)
            {
                emitter.Ldind(this.Type);
            }

            // object -> T
            if (destArg.Access == ParameterAccess.Default)
            {
                // object -> struct
                if (destArg.Type.IsValueType)
                {
                    emitter.Unbox_Any(destArg.Type);
                }
                // object -> class
                else
                {
                    emitter.Castclass(destArg.Type);
                }
            }
            // object -> ref T
            else
            {
                // object -> ref struct
                if (destArg.Type.IsValueType)
                {
                    emitter.Unbox(destArg.Type);
                }
                // object -> ref class
                else
                {
                    emitter.Castclass(destArg.Type)
                        .DeclareLocal(destArg.Type, out var localDest)
                        .Stloc(localDest)
                        .Ldloca(localDest);
                }
            }

            // Done
            return true;
        }

        // ?T -> ?T
        if (this.Type == destArg.Type)
        {
            // T -> ?T
            if (this.Access == ParameterAccess.Default)
            {
                // T -> T
                if (this.Access == ParameterAccess.Default)
                {
                    // Ensure value is on stack
                    Load(emitter);
                }
                // T -> ref T
                else
                {
                    // Load if I have to
                    if (!IsOnStack)
                    {
                        LoadAddress(emitter);
                    }
                    else
                    {
                        emitter.DeclareLocal(this.Type, out var localSource)
                            .Stloc(localSource)
                            .Ldloca(localSource);
                    }
                }
            }
            // ref T -> ?T
            else
            {
                // ref T -> T
                if (destArg.Access == ParameterAccess.Default)
                {
                    // Ensure value is on stack
                    Load(emitter);

                    emitter.Ldind(this.Type);
                }
                // ref T -> ref T
                else
                {
                    // Ensure value is on stack
                    Load(emitter);
                }
            }

            // done
            return true;
        }

        /* ?T:U -> ?U
         * Tests for implements, so we can autocast
         * This also takes care of interfaces
         */
        if (this.Type.Implements(destArg.Type))
        {
            // T:U -> U
            if (this.Access != ParameterAccess.Default || destArg.Access != ParameterAccess.Default)
                return GetNotImplementedEx();

            // struct T:U -> U
            if (this.Type.IsValueType)
            {
                // We have to be converting to an interface
                if (!destArg.Type.IsInterface)
                    throw new InvalidOperationException();
                Debugger.Break();

                // Ensure value is on stack
                Load(emitter);

                // this works?
                emitter.Castclass(destArg.Type);
            }
            // class T:U -> U
            else
            {
                // Ensure value is on stack
                Load(emitter);

                emitter.Castclass(destArg.Type);
            }

            // done
            return true;
        }

        // We don't know how to do this (yet)
        return GetNotImplementedEx();
    }

    public virtual Type ToType()
    {
        if (Access != ParameterAccess.Default)
        {
            return this.Type.MakeByRefType();
        }
        else
        {
            return this.Type;
        }
    }

    public bool Equals(Arg? arg)
    {
        return arg is not null &&
               arg.Access == this.Access &&
               arg.Type == this.Type;
    }

    public sealed override bool Equals(object? obj)
    {
        return obj is Arg arg && Equals(arg);
    }

    public sealed override int GetHashCode()
    {
        return Hasher.GetHashCode(Access, Type);
    }

    public override string ToString()
    {
        if (Access == ParameterAccess.Default)
            return Dump(Type);
        return Dump($"{Access} {Type}");
    }
}