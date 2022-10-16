using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Jay.Enums;
using Jay.Reflection.Exceptions;

namespace Jay.Reflection.Building.Backing;

public interface IEqualsBacker
{
    (MethodInfo EqualMethod, MethodInfo GetHashCodeMethod) CreateEqualityMethods(
        IReadOnlyDictionary<PropertyInfo, (FieldInfo, PropertyInfo)> implementedProperties);
}

public interface IToStringBacker
{
    MethodInfo CreateToStringMethod(
        IReadOnlyDictionary<PropertyInfo, (FieldInfo, PropertyInfo)> implementedProperties);
}

public class InterfaceImplementer
{
    public static Type CreateImplementationType<TInterface>() where TInterface : class
    {
        return new InterfaceImplementer(typeof(TInterface)).CreateImplementingType();
    }

    protected readonly HashSet<Type> _interfaces;
    protected readonly TypeBuilder _typeBuilder;

    protected readonly Dictionary<string, FieldBuilder> _builtFields = new(StringComparer.OrdinalIgnoreCase);
    protected readonly Dictionary<string, PropertyBuilder> _builtProperties = new(StringComparer.OrdinalIgnoreCase);
    protected readonly Dictionary<string, EventBuilder> _builtEvents = new(StringComparer.OrdinalIgnoreCase);
    protected readonly Dictionary<string, ConstructorBuilder> _builtConstructors = new(StringComparer.OrdinalIgnoreCase);
    protected readonly Dictionary<string, MethodBuilder> _builtMethods = new(StringComparer.OrdinalIgnoreCase);

    public Type InterfaceType { get; }

    public InterfaceImplementer(Type interfaceType)
    {
        ArgumentNullException.ThrowIfNull(interfaceType);
        if (!interfaceType.IsInterface)
            throw new ArgumentException("InterfaceBacker must be passed an interface type", nameof(interfaceType));
        this.InterfaceType = interfaceType;

        _interfaces = new HashSet<Type>(InterfaceType.GetInterfaces())
        {
            interfaceType
        };
        _typeBuilder = RuntimeBuilder.DefineType(
            TypeAttributes.Public | TypeAttributes.Class,
            MemberNaming.CreateInterfaceImplementationName(InterfaceType));
    }

    private void ImplementProperties()
    {
        IBackingFieldImplementer backingFieldImplementer = new BackingFieldImplementer(_typeBuilder);
        IPropertyGetMethodImplementer getMethodImplementer = new DefaultInstancePropertyGetMethodImplementer(_typeBuilder);
        IPropertySetMethodImplementer setMethodImplementer;
        var notifyPropertyChanging = _interfaces!.Contains(typeof(INotifyPropertyChanging));
        var notifyPropertyChanged = _interfaces!.Contains(typeof(INotifyPropertyChanged));
        if (notifyPropertyChanging || notifyPropertyChanged)
        {
            setMethodImplementer = new NotifyPropertySetMethodImplementer(_typeBuilder,
                null,
                null);
            throw new NotImplementedException();
        }
        else
        {
            setMethodImplementer = new DefaultInstancePropertySetMethodImplementer(_typeBuilder);
        }

        IPropertyImplementer propertyImplementer = new PropertyImplementer(_typeBuilder,
            backingFieldImplementer,
            getMethodImplementer,
            setMethodImplementer);

        var properties = _interfaces
            .SelectMany(face => face.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .ToList();

        foreach (var property in properties)
        {
            if (_builtProperties.ContainsKey(property.Name)) continue;
            var pack = propertyImplementer.ImplementProperty(property);
            _builtFields.Add(pack.BackingField.Name, pack.BackingField);
            if (pack.GetMethod is not null)
            {
                _builtMethods.Add(pack.GetMethod.Name, pack.GetMethod);
            }

            if (pack.SetMethod is not null)
            {
                _builtMethods.Add(pack.SetMethod.Name, pack.SetMethod);
            }

            _builtProperties.Add(pack.Property.Name, pack.Property);
        }
    }

    private void ImplementEvents()
    {
    }

    private void ImplementMethods()
    {
        var methods = _interfaces
            .SelectMany(face => face.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            .Where(method => !method.Attributes.HasAnyFlags(MethodAttributes.SpecialName))
            .ToList();
        foreach (var method in methods)
        {
            if (_builtMethods.ContainsKey(method.Name)) continue;

            // Has a default implementation?
            if (!method.Attributes.HasAnyFlags(MethodAttributes.Abstract))
            {
                var implr = new InterfaceDefaultMethodImplementer(_typeBuilder);
                var methodBuilder = implr.ImplementMethod(method);
                _builtMethods.Add(method.Name, methodBuilder);
                continue;
            }

            // Equals<T> ?
            if (method.Name == "Equals" && method.GetParameterTypes()[0] == InterfaceType)
            {
                var implr = new EqualsMethodsImplementer(_typeBuilder);
                Debugger.Break();
            }

            throw new NotImplementedException();
        }
    }

    private void ImplementConstructor()
    {
        MethodAttributes attr = MethodAttributes.Public;
        if (_typeBuilder.IsStatic())
            attr |= MethodAttributes.Static;
        _typeBuilder.DefineDefaultConstructor(attr);
    }

    public Type CreateImplementingType()
    {
        // Implement all attributes
        AttributeImplementer.ImplementAttributes(InterfaceType, _typeBuilder.SetCustomAttribute);

        // We inherit from object, as everything should
        _typeBuilder.SetParent(typeof(object));

        // Events (sets up INotifyPropertyXXX)
        ImplementEvents();

        // Properties
        ImplementProperties();

        // Methods
        ImplementMethods();

        // Constructor
        ImplementConstructor();

        // Implemented all interfaces
        _interfaces.Consume(face => _typeBuilder.AddInterfaceImplementation(face));


        // Create our type
        try
        {
            var implType = _typeBuilder.CreateType();
            if (implType is null)
                throw new ReflectionException($"Unable to create an implementation for {InterfaceType}");
            return implType;
        }
        catch (Exception ex)
        {
            throw new ReflectionException($"Unable to create an implementation for {InterfaceType}", ex);
        }
    }
}