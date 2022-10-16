using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public interface IFieldImplementer
{
    FieldBuilder ImplementField(FieldInfo field);
}