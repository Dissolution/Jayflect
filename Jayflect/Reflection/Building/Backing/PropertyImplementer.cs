using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public class PropertyImplementer : Implementer, IPropertyImplementer
{
    protected readonly IBackingFieldImplementer _backingFieldImplementer;
    protected readonly IPropertyGetMethodImplementer _getMethodImplementer;
    protected readonly IPropertySetMethodImplementer _propertySetMethodImplementer;

    public PropertyImplementer(TypeBuilder typeBuilder,
        IBackingFieldImplementer backingFieldImplementer, 
        IPropertyGetMethodImplementer getMethodImplementer, 
        IPropertySetMethodImplementer propertySetMethodImplementer)
        : base(typeBuilder)
    {
        _backingFieldImplementer = backingFieldImplementer;
        _getMethodImplementer = getMethodImplementer;
        _propertySetMethodImplementer = propertySetMethodImplementer;
    }

    public virtual PropertyImpl ImplementProperty(PropertyInfo property)
    {
        var fieldBuilder = _backingFieldImplementer.ImplementBackingField(property);
        var parameterTypes = property.GetIndexParameterTypes();
        var propertyBuilder = _typeBuilder.DefineProperty(
            property.Name,
            PropertyAttributes.None,
            GetCallingConventions(property),
            property.PropertyType,
            parameterTypes);
        AttributeImplementer.ImplementAttributes(property, propertyBuilder.SetCustomAttribute);
        // Getter?
        MethodBuilder? getMethodBuilder = null;
        if (property.CanRead)
        {
            getMethodBuilder = _getMethodImplementer.ImplementGetMethod(fieldBuilder, propertyBuilder);
        }
        // Setter?
        MethodBuilder? setMethodBuilder = null;
        if (property.CanWrite)
        {
            setMethodBuilder = _propertySetMethodImplementer.ImplementSetMethod(fieldBuilder, propertyBuilder);
        }
        
        // Implemented everything backing this Property
        return new PropertyImpl
        (
            fieldBuilder,
            getMethodBuilder,
            setMethodBuilder,
            propertyBuilder
        );
    }
}