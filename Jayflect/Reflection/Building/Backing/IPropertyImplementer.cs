using System.Reflection;

namespace Jay.Reflection.Building.Backing;

public interface IPropertyImplementer
{
    PropertyImpl ImplementProperty(PropertyInfo property);
}