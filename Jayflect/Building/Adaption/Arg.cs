#pragma warning disable CS0659, CS0660, CS0661

using System.Diagnostics;
using Jay;
using Jay.Extensions;
using Jayflect.Building.Emission;
using Jayflect.Exceptions;


namespace Jayflect.Building.Adaption;

public abstract class Arg : IEquatable<Arg>
{
    public static implicit operator Arg(ParameterInfo parameter) => new ParameterArg(parameter);
    public static implicit operator Arg(Type type) => new StackArg(type);
    public static implicit operator Arg(EmitterLocal local) => new LocalArg(local);
   
    public static bool operator ==(Arg left, Arg right) => left.Equals(right);
    public static bool operator !=(Arg left, Arg right) => !left.Equals(right);

    public static Arg FromParameter(ParameterInfo parameter) => new ParameterArg(parameter);
    public static Arg FromType(Type type) => new StackArg(type);
    public static Arg FromLocal(EmitterLocal local) => new LocalArg(local);
   
    
    public Type Type { get; }
    public bool IsByRef { get; }
    public Type UnderType { get; }
    
    public bool IsOnStack { get; }
    public bool PreferNonRef { get; }
    
    protected Arg(Type type, bool isOnStack, bool preferNonRef)
    {
        this.Type = type;
        if (type.IsByRef)
        {
            IsByRef = true;
            UnderType = type.GetElementType()!;
        }
        else
        {
            IsByRef = false;
            UnderType = type;
        }
        this.IsOnStack = isOnStack;
        this.PreferNonRef = preferNonRef;
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
        if (UnderType.IsPointer || destArg.UnderType.IsPointer)
            return GetNotImplementedEx();

        // ? -> void
        if (destArg.UnderType == typeof(void))
        {
            if (destArg.IsByRef)
                return GetNotImplementedEx();

            // Source is also void?
            if (UnderType == typeof(void))
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

        if (UnderType == typeof(void))
        {
            // Creating a value isn't great
            exactness = 10;
            return true;
        }

        /* ? -> ?object
         * Needs to be checked early or it will be caught up in the
         * .Implements() check below
         */
        if (destArg.UnderType == typeof(object))
        {
            // ?T -> object
            if (destArg.IsByRef)
                return GetNotImplementedEx();

            // Boxing is fine
            exactness = 5;
            return true;
        }

        /* ?object -> ?
         * Unboxing
         */
        if (this.UnderType == typeof(object))
        {
            // Unboxing is fine
            exactness = 5;
            return true;
        }

        // ?T -> ?T
        if (this.UnderType == destArg.UnderType)
        {
            // Exact is great
            exactness = 0;
            return true;
        }

        /* ?T:U -> ?U
         * Tests for implements, so we can autocast
         * This also takes care of interfaces
         */
        if (this.UnderType.Implements(destArg.UnderType))
        {
            // T:U -> U
            if (this.IsByRef || destArg.IsByRef)
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
        if (UnderType.IsPointer || destArg.UnderType.IsPointer)
            return GetNotImplementedEx();

        
        
        // ? -> void
        if (destArg.UnderType == typeof(void))
        {
            if (destArg.IsByRef)
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
        if (UnderType == typeof(void))
        {
            if (!destArg.IsByRef)
            {
                emitter.LoadDefault(destArg.UnderType);
            }
            else
            {
                emitter.LoadDefaultAddress(destArg.UnderType);
            }

            // done
            return true;
        }

        /* ? -> ?object
         * Needs to be checked early or it will be caught up in the
         * .Implements() check below
         */
        if (destArg.UnderType == typeof(object))
        {
            // ?T -> object
            if (destArg.IsByRef)
                return GetNotImplementedEx();

            // We need to get a boxed value

            // Ensure value is on stack
            Load(emitter);

            // ref T -> object
            if (this.IsByRef)
            {
                // get the T
                emitter.Ldind(this.UnderType);
            }

            // If we're not already typeof(object), box us
            if (this.UnderType != typeof(object))
            {
                emitter.Box(this.UnderType);
            }

            return true;
        }

        /* ?object -> ?
         * Unboxing
         */
        if (this.UnderType == typeof(object))
        {
            // We need to unbox a value

            if (emitTypeChecks)
            {
                return GetNotImplementedEx();
            }

            // Ensure value is on stack
            Load(emitter);

            // ref object -> object
            if (this.IsByRef)
            {
                emitter.Ldind(this.UnderType);
            }

            // object -> T
            if (!destArg.IsByRef)
            {
                // object -> struct
                if (destArg.UnderType.IsValueType)
                {
                    emitter.Unbox_Any(destArg.UnderType);
                }
                // object -> class
                else
                {
                    emitter.Castclass(destArg.UnderType);
                }
            }
            // object -> ref T
            else
            {
                // object -> ref struct
                if (destArg.UnderType.IsValueType)
                {
                    emitter.Unbox(destArg.UnderType);
                }
                // object -> ref class
                else
                {
                    emitter.Castclass(destArg.UnderType)
                        .DeclareLocal(destArg.UnderType, out var localDest)
                        .Stloc(localDest)
                        .Ldloca(localDest);
                }
            }

            // Done
            return true;
        }

        // ?T -> ?T
        if (this.UnderType == destArg.UnderType)
        {
            // T -> ?T
            if (!this.IsByRef)
            {
                // T -> T
                if (!destArg.IsByRef)
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
                        emitter.DeclareLocal(this.UnderType, out var localSource)
                            .Stloc(localSource)
                            .Ldloca(localSource);
                    }
                }
            }
            // ref T -> ?T
            else
            {
                // ref T -> T
                if (!destArg.IsByRef)
                {
                    // Ensure value is on stack
                    Load(emitter);

                    emitter.Ldind(this.UnderType);
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
        if (this.UnderType.Implements(destArg.UnderType))
        {
            // T:U -> U
            if (this.IsByRef || destArg.IsByRef)
                return GetNotImplementedEx();

            // struct T:U -> U
            if (this.UnderType.IsValueType)
            {
                // We have to be converting to an interface
                if (!destArg.UnderType.IsInterface)
                    throw new InvalidOperationException();
                Debugger.Break();

                // Ensure value is on stack
                Load(emitter);

                // this works?
                emitter.Castclass(destArg.UnderType);
            }
            // class T:U -> U
            else
            {
                // Ensure value is on stack
                Load(emitter);

                emitter.Castclass(destArg.UnderType);
            }

            // done
            return true;
        }

        // We don't know how to do this (yet)
        return GetNotImplementedEx();
    }

    public abstract bool Equals(Arg? arg);

    public sealed override bool Equals(object? obj)
    {
        return obj is Arg arg && Equals(arg);
    }

    // Remember: have to override GetHashCode() in implementations

    // Same for ToString()
}