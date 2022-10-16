using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Jay.Enums;

namespace Jay.Reflection.Building.Backing;

public interface IMethodImplementer
{
    MethodBuilder ImplementMethod(MethodInfo method);
}

internal class MethodImplementer : Implementer, IMethodImplementer
{
    public MethodImplementer(TypeBuilder typeBuilder) : base(typeBuilder)
    {
    }

    public MethodBuilder ImplementMethod(MethodInfo method)
    {
        throw new NotImplementedException();
    }
}

internal class InterfaceDefaultMethodImplementer : Implementer, IMethodImplementer
{
    public InterfaceDefaultMethodImplementer(TypeBuilder typeBuilder) : base(typeBuilder)
    {
    }

    public MethodBuilder ImplementMethod(MethodInfo method)
    {
        // Has an implementation
        Debug.Assert(!method.Attributes.HasAnyFlags(MethodAttributes.Abstract));
        // Implementation is always HasThis!
        
        var methodBuilder = _typeBuilder.DefineMethod(
            method.Name,
            GetMethodImplementationAttributes(method) | MethodAttributes.HideBySig,
            GetCallingConventions(method),
            method.ReturnType,
            method.GetParameterTypes());
        var emitter = methodBuilder.GetILEmitter();
        // Load this
        emitter.Ldarg_0()
            .Castclass(method.DeclaringType!);      // as Interface
        int len = method.GetParameters().Length;
        // Load all the rest of the args
        for (var i = 1; i <= len; i++)
        {
            emitter.Ldarg(i);
        }
        // Call the interface's method (not virtual!)
        emitter.Emit(OpCodes.Call, method)
            .Ret();

        return methodBuilder;
    }
}

public sealed record class EqualsMethodsImpl
(
    MethodBuilder EqualsTMethod,
    MethodBuilder EqualsObjMethod,
    MethodBuilder GetHashCodeMethod
);

internal class EqualsMethodsImplementer : Implementer
{
    public EqualsMethodsImplementer(TypeBuilder typeBuilder) 
        : base(typeBuilder)
    {
    }

    public EqualsMethodsImpl ImplementEquals(Type instanceType)
    {
        var equalsTMethodBuilder = _typeBuilder.DefineMethod(
            "Equals",
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final,
            CallingConventions.HasThis,
            typeof(bool),
            new Type[1] { instanceType });
        
        // Equals<T> is really Ceq/RefEquals by default
        // We have to use EqualityAttribute in order to specify that we want more advanced behavior
        
        


        equalsTMethodBuilder.Emit(emitter =>
        {
            /* public bool Equals(T? other)
             * {
             *   if (other is null) return false;
             *   if (other._field1 != this._field1) return false;
             *   ...
             *   if (other._fieldN != this._fieldN) return false;
             *   return true;
             * }
             */
            emitter.Ldarg_1()
                .Brfalse(out var lblRetFalse)
                
                
                
                
                
                
                .MarkLabel(lblRetFalse)
                .Ldc_I4_0()
                .Ret();
            ;
        });
        throw new NotImplementedException();
    }
}