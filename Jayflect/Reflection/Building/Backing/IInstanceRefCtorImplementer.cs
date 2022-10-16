using System.Reflection;

namespace Jay.Reflection.Building.Backing;

public interface IInstanceRefCtorImplementer
{
    ConstructorImpl ImplementInstanceReferenceConstructor(ConstructorInfo ctor);
}