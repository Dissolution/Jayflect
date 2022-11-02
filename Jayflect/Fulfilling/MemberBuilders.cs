/*using System.Reflection;
using System.Reflection.Emit;

namespace Jay.Reflection.Implementation;

public sealed class MemberBuilders
{
    // FieldInfo -> FieldBuilder
    private readonly Dictionary<FieldInfo, FieldBuilder> _fieldBuilders;
    // PropertyInfo -> PropertyBuilder, FieldBuilder, 2x MethodBuilder
    

    private readonly Dictionary<MemberInfo, MemberInfo> _memberBuilders;
    private readonly Dictionary<EventInfo, EventBuilder> _eventBuilders;

    public MemberBuilders()
    {
        _memberBuilders = new Dictionary<MemberInfo, MemberInfo>(
            MemberSignatureEqualityComparer.Instance);
        _eventBuilders = new Dictionary<EventInfo, EventBuilder>(
            MemberSignatureEqualityComparer.Instance);
    }

    public bool TryAdd(FieldInfo field, FieldBuilder fieldBuilder)
    {
        return _memberBuilders.TryAdd(field, fieldBuilder);
    }
    
    public bool TryAdd(PropertyInfo property, PropertyBuilder propertyBuilder)
    {
        return _memberBuilders.TryAdd(property, propertyBuilder);
    }
    
    public bool TryAdd(EventInfo @event, EventBuilder eventBuilder)
    {
        return _eventBuilders.TryAdd(@event, eventBuilder);
    }
    
    public bool TryAdd(ConstructorInfo constructor, ConstructorBuilder ctorBuilder)
    {
        return _memberBuilders.TryAdd(constructor, ctorBuilder);
    }
    
    public bool TryAdd(MethodInfo method, MethodBuilder methodBuilder)
    {
        return _memberBuilders.TryAdd(method, methodBuilder);
    }
    
    public bool Contains(MemberInfo member)
    {
        if (member is EventInfo @event)
            return _eventBuilders.ContainsKey(@event);
        return _memberBuilders.ContainsKey(member);
    }

    public bool TryGetField(FieldInfo field, [NotNullWhen(true)] out FieldBuilder? fieldBuilder)
    {
        if (_memberBuilders.TryGetValue(field, out var builder))
        {
            return builder.Is(out fieldBuilder);
        }

        fieldBuilder = null;
        return false;
    }
    
    public bool TryGetProperty(PropertyInfo property, [NotNullWhen(true)] out PropertyBuilder? propertyBuilder)
    {
        if (_memberBuilders.TryGetValue(property, out var builder))
        {
            return builder.Is(out propertyBuilder);
        }

        propertyBuilder = null;
        return false;
    }
    
    public bool TryGetEvent(EventInfo @event, [NotNullWhen(true)] out EventBuilder? eventBuilder)
    {
        return _eventBuilders.TryGetValue(@event, out eventBuilder);
    }
    
    public bool TryGetConstructor(ConstructorInfo constructor, [NotNullWhen(true)] out ConstructorBuilder? ctorBuilder)
    {
        if (_memberBuilders.TryGetValue(constructor, out var builder))
        {
            return builder.Is(out ctorBuilder);
        }

        ctorBuilder = null;
        return false;
    }
    
    public bool TryGetMethod(MethodInfo method, [NotNullWhen(true)] out MethodBuilder? methodBuilder)
    {
        if (_memberBuilders.TryGetValue(method, out var builder))
        {
            return builder.Is(out methodBuilder);
        }

        methodBuilder = null;
        return false;
    }
}*/