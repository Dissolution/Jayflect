using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public interface IBackingFieldImplementer
{
    FieldBuilder ImplementBackingField(PropertyInfo property);
}