using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public interface IConstructorImplementer
{
    ConstructorBuilder ImplementConstructor(ConstructorInfo ctor);
    ConstructorBuilder ImplementDefaultConstructor(MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.SpecialName);
}