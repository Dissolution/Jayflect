/*
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Jay.Reflection.Extensions;
using Jay.Text;

namespace Jay.Reflection.Building.Fulfilling;

public partial class Fulfiller
{
    public abstract class Fulfilled
    {
        
    }

    public class FulfilledProperty : Fulfilled
    {
        public PropertyBuilder Property { get; init; }
        public FieldBuilder BackingField { get; init; }
        public MethodBuilder Getter { get; init; }
        public MethodBuilder Setter { get; init; }
    }

    public class FulfilledEvent : Fulfilled
    {
        public EventBuilder Event { get; init; }
        public FieldBuilder BackingField { get; init; }
        public MethodBuilder Adder { get; init; }
        public MethodBuilder Remover { get; init; }
        public MethodBuilder Caller { get; init; }
    }
}

public partial class Fulfiller
{
    private readonly TypeBuilder _typeBuilder;
    private readonly IReadOnlySet<Type> _interfaceTypes;
    private readonly Dictionary<MemberInfo, Fulfilled> _fulfilledMembers;

    private static string GetBackingFieldName(MemberInfo member)
    {
        return string.Create(member.Name.Length + 1, member.Name, (span, name) =>
        {
            span[0] = '_';
            span[1] = char.ToLower(name[0]);
            TextHelper.CopyTo(name.AsSpan(1), span[2..]);
        });
    }

    private static string GetPropertyGetterName(PropertyInfo property)
    {
        return $"get_{property.Name}";
    }
    
    private static string GetPropertySetterName(PropertyInfo property)
    {
        return $"set_{property.Name}";
    }

    protected void EmitPropertyGetter(MethodBuilder getter)
    {
        
    }
    
    protected void EmitPropertySetter(MethodBuilder setter)
    {
        /* set => {
         *  if (_field != value)
         *  {
         *      this.NotifyPropertyChanging?();
         *      _field = value;
         *      this.NotifyPropertyChanged?();
         *  }};
         * #1#
        
        var emitter = setter.GetEmitter();
        
    }

    protected readonly Dictionary<PropertyInfo, FulfilledProperty> _fulfilledProperties;

    protected FulfilledProperty FulfillProperty(PropertyInfo property)
    {
        return _fulfilledProperties.GetOrAdd(property, prop =>
        {
            Type[]? parameterTypes;
            var indexParameters = prop.GetIndexParameters();
            if (indexParameters.Length > 0)
            {
                Debugger.Break();
                parameterTypes = new Type[indexParameters.Length];
                for (var i = 0; i < indexParameters.Length; i++)
                {
                    parameterTypes[i] = indexParameters[i].ParameterType;
                }
            }
            else
            {
                parameterTypes = null;
            }

            var fField = _typeBuilder.DefineField(fieldName: GetBackingFieldName(prop),
                                                 type: prop.PropertyType,
                                                 FieldAttributes.Private);

            var fProperty = _typeBuilder.DefineProperty(name: prop.Name,
                                                   attributes: prop.Attributes,
                                                   callingConvention: CallingConventions.HasThis,
                                                   returnType: prop.PropertyType,
                                                   returnTypeRequiredCustomModifiers: null,
                                                   returnTypeOptionalCustomModifiers: null,
                                                   parameterTypes: parameterTypes,
                                                   parameterTypeRequiredCustomModifiers: null,
                                                   parameterTypeOptionalCustomModifiers: null);
            var fGetter = _typeBuilder.DefineMethod(name: GetPropertyGetterName(prop),
                                                    attributes: MethodAttributes.Private | MethodAttributes.Final,
                                                    callingConvention: CallingConventions.HasThis,
                                                    returnType: prop.PropertyType,
                                                    parameterTypes: Array.Empty<Type>());
            EmitPropertyGetter(fGetter);
            var fSetter = _typeBuilder.DefineMethod(name: GetPropertySetterName(prop),
                                                       attributes: MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.SpecialName,
                                                       callingConvention: CallingConventions.HasThis,
                                                       returnType: typeof(void),
                                                       parameterTypes: new Type[1]{prop.PropertyType});
            EmitPropertySetter(fSetter);

            return new FulfilledProperty
            {
                Property = fProperty,
                BackingField = fField,
                Getter = fGetter,
                Setter = fSetter,
            };
        });
    }
}
*/

