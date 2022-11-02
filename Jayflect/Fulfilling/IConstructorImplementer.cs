namespace Jayflect.Fulfilling;

public interface IConstructorImplementer
{
    ConstructorBuilder ImplementConstructor(ConstructorInfo ctor);
    ConstructorBuilder ImplementDefaultConstructor(MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.SpecialName);
}