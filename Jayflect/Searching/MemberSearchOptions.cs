using Jayflect.Building.Adaption;

namespace Jayflect.Searching;



public sealed class MemberSearchOptions
{
    public Visibility Visibility { get; init; } = Visibility.Any;
    public string? Name { get; init; } = null;
    public NameMatchOptions NameMatch { get; init; } = NameMatchOptions.Exact;
    public Type? ReturnType { get; init; } = null;
    public Type[]? ParameterTypes { get; init; } = null;
    public bool ConvertableTypeMatch { get; init; } = false;

    public MemberSearchOptions()
    {
        
    }
    
    public MemberSearchOptions(Visibility visibility)
    {
        this.Visibility = visibility;
    }
    public MemberSearchOptions(string? name, Visibility visibility)
    {
        this.Name = name;
        this.Visibility = visibility;
    }
    public MemberSearchOptions(string? name, Visibility visibility, Type type)
    {
        this.Name = name;
        this.Visibility = visibility;
        this.ReturnType = type;
    }
    
    public MemberSearchOptions(string? name, Visibility visibility, Type returnType, params Type[] parameterTypes)
    {
        this.Name = name;
        this.Visibility = visibility;
        this.ReturnType = returnType;
        this.ParameterTypes = parameterTypes;
    }
    
    
    private bool MatchName(string memberName)
    {
        if (Name is null) return true;
        if (memberName.Length < Name.Length) return false;
        if (NameMatch == NameMatchOptions.Exact)
            return string.Equals(memberName, this.Name);
        if (NameMatch.HasFlag(NameMatchOptions.IgnoreCase))
        {
            if (NameMatch.HasFlag(NameMatchOptions.Contains))
                return memberName.Contains(this.Name, StringComparison.OrdinalIgnoreCase);
            
            if (NameMatch.HasFlag(NameMatchOptions.StartsWith))
            {
                if (memberName.StartsWith(this.Name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            if (NameMatch.HasFlag(NameMatchOptions.EndsWith))
            {
                if (memberName.EndsWith(this.Name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            return string.Equals(memberName, this.Name, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            if (NameMatch.HasFlag(NameMatchOptions.Contains))
                return memberName.Contains(this.Name);
            
            if (NameMatch.HasFlag(NameMatchOptions.StartsWith))
            {
                if (memberName.StartsWith(this.Name))
                    return true;
            }
            
            if (NameMatch.HasFlag(NameMatchOptions.EndsWith))
            {
                if (memberName.EndsWith(this.Name))
                    return true;
            }
            
            return string.Equals(memberName, this.Name);

        }
    }
    private bool MatchReturnType(Type returnType)
    {
        if (ReturnType is null) return true;
        if (ConvertableTypeMatch)
        {
            if (!RuntimeMethodAdapter.CanAdaptType(returnType, ReturnType))
                return false;
        }
        else
        {
            if (returnType != ReturnType)
                return false;
        }
        return true;
    }
    private bool MatchParameterType(Type sourceType, Type destType)
    {
        if (ConvertableTypeMatch)
        {
            if (!RuntimeMethodAdapter.CanAdaptType(sourceType, destType))
                return false;
        }
        else
        {
            if (sourceType != destType)
                return false;
        }
        return true;
    }
    private bool MatchParameterTypes(Type[] argTypes)
    {
        if (ParameterTypes is null) return true;
        if (ParameterTypes.Length != argTypes.Length) return false;
        for (var i = 0; i < argTypes.Length; i++)
        {
            if (!MatchParameterType(ParameterTypes[i], argTypes[i]))
                return false;
        }
        return true;
    }
    
    public bool Matches(MemberInfo member)
    {
        // We always know visibility matches, as we passed it to get this member
        // TODO: DEBUG THIS

        if (!MatchName(member.Name)) return false;

        if (member is FieldInfo field)
        {
            if (!MatchReturnType(field.FieldType)) return false;
            if (!MatchParameterTypes(Type.EmptyTypes)) return false;
            return true;
        }
        
        if (member is PropertyInfo property)
        {
            if (!MatchReturnType(property.PropertyType)) return false;
            if (!MatchParameterTypes(property.GetIndexParameterTypes())) return false;
            return true;
        }
        
        if (member is EventInfo eventInfo)
        {
            if (!MatchReturnType(eventInfo.EventHandlerType!)) return false;
            if (!MatchParameterTypes(Type.EmptyTypes)) return false;
            return true;
        }
        
        if (member is ConstructorInfo ctor)
        {
            if (!MatchReturnType(ctor.DeclaringType!)) return false;
            if (!MatchParameterTypes(ctor.GetParameterTypes())) return false;
                return true;
        }
        
        if (member is MethodInfo method)
        {
            if (!MatchReturnType(method.ReturnType!)) return false;
            if (!MatchParameterTypes(method.GetParameterTypes())) return false;
            return true;
        }

        throw new NotImplementedException();
        //return false;
    }
}