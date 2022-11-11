using System.Diagnostics;
using Jayflect.Building.Adaption;

namespace Jayflect.Searching;

public sealed record class MemberSearchOptions
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
    
    public MemberSearchOptions(
        string? name = null, 
        Visibility visibility = Visibility.Any, 
        Type? returnType = null, 
        params Type[]? parameterTypes)
    {
        this.Name = name;
        this.Visibility = visibility;
        this.ReturnType = returnType;
        this.ParameterTypes = parameterTypes;
    }
    
    
    private bool MatchesName(string memberName)
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
    private bool MatchesReturnType(Type returnType)
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
    private bool MatchesParameterType(Type sourceType, Type destType)
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
    private bool MatchesParameterTypes(Type[] argTypes)
    {
        if (ParameterTypes is null) return true;
        if (ParameterTypes.Length != argTypes.Length) return false;
        for (var i = 0; i < argTypes.Length; i++)
        {
            if (!MatchesParameterType(ParameterTypes[i], argTypes[i]))
                return false;
        }
        return true;
    }
    
    public bool Matches(MemberInfo member)
    {
        // We always know visibility matches, as we passed it to get this member
        var f = Visibility.HasAll(member.GetVisibility());
        Debugger.Break();

        if (!MatchesName(member.Name)) return false;

        if (member is FieldInfo field)
        {
            if (!MatchesReturnType(field.FieldType)) return false;
            if (!MatchesParameterTypes(Type.EmptyTypes)) return false;
            return true;
        }
        
        if (member is PropertyInfo property)
        {
            if (!MatchesReturnType(property.PropertyType)) return false;
            if (!MatchesParameterTypes(property.GetIndexParameterTypes())) return false;
            return true;
        }
        
        if (member is EventInfo eventInfo)
        {
            if (!MatchesReturnType(eventInfo.EventHandlerType!)) return false;
            if (!MatchesParameterTypes(Type.EmptyTypes)) return false;
            return true;
        }
        
        if (member is ConstructorInfo ctor)
        {
            if (!MatchesReturnType(ctor.DeclaringType!)) return false;
            if (!MatchesParameterTypes(ctor.GetParameterTypes())) return false;
            return true;
        }
        
        if (member is MethodInfo method)
        {
            if (!MatchesReturnType(method.ReturnType!)) return false;
            if (!MatchesParameterTypes(method.GetParameterTypes())) return false;
            return true;
        }

        throw new NotImplementedException();
    }
}