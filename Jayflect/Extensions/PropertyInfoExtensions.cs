using System.Reflection.Emit;

namespace Jayflect.Extensions;

public static class PropertyInfoExtensions
{
    public static MethodInfo? GetGetter(this PropertyInfo? propertyInfo)
    {
        if (propertyInfo is null) return null;
        return propertyInfo.GetGetMethod(false) ??
               propertyInfo.GetGetMethod(true);
    }
        
    public static MethodInfo? GetSetter(this PropertyInfo? propertyInfo)
    {
        if (propertyInfo is null) return null;
        return propertyInfo.GetSetMethod(false) ??
               propertyInfo.GetSetMethod(true);
    }
        
    public static Visibility Visibility(this PropertyInfo? propertyInfo)
    {
        Visibility visibility = Jayflect.Visibility.None;
        if (propertyInfo is null)
            return visibility;
        visibility |= propertyInfo.GetGetter().Visibility();
        visibility |= propertyInfo.GetSetter().Visibility();
        return visibility;
    }

    public static bool IsStatic(this PropertyInfo? propertyInfo)
    {
        if (propertyInfo is null)
            return false;
        return propertyInfo.GetGetter().IsStatic() ||
               propertyInfo.GetSetter().IsStatic();
    }

    public static Type[] GetIndexParameterTypes(this PropertyInfo property)
    {
        var indexerParams = property.GetIndexParameters();
        var len = indexerParams.Length;
        if (len == 0) return Type.EmptyTypes;
        var types = new Type[len];
        for (var i = 0; i < len; i++)
        {
            types[i] = indexerParams[i].ParameterType;
        }
        return types;
    }
    
    private static string GetBackingFieldName(PropertyInfo property) => $"<{property.Name}>k__BackingField";

    public static FieldInfo? GetBackingField(this PropertyInfo? propertyInfo)
    {
        var ownerType = propertyInfo?.DeclaringType;
        if (ownerType is null) return null;
        var flags = BindingFlags.NonPublic;
        flags |= propertyInfo.IsStatic() ? BindingFlags.Static : BindingFlags.Instance;
        var field = ownerType.GetField(GetBackingFieldName(propertyInfo!), flags);
        /*
        if (field is null)
        {
            var getter = propertyInfo.GetGetter();
            if (getter is not null)
            {
                field = getter.GetInstructions()
                              .Where(inst =>
                                  inst.OpCode == OpCodes.Ldfld || inst.OpCode == OpCodes.Ldflda ||
                                  inst.OpCode == OpCodes.Ldsfld || inst.OpCode == OpCodes.Ldsflda)
                              .SelectWhere((Instruction inst, [NotNullWhen(true)] out FieldInfo? fld) =>
                              {
                                  if (inst.Arg.Is(out fld) &&
                                      fld.DeclaringType == owner &&
                                      fld.FieldType == propertyInfo.PropertyType)
                                  {
                                      return true;
                                  }

                                  fld = null;
                                  return false;
                              })
                              .OrderBy(fld => Levenshtein.Calculate(fld!.Name, propertyInfo.Name))
                              .FirstOrDefault();
            }
        }

        if (field is null)
        {
            var setter = propertyInfo.GetSetter();
            if (setter is not null)
            {
                field = setter.GetInstructions()
                              .Where(inst => inst.OpCode == OpCodes.Stfld || inst.OpCode == OpCodes.Stsfld)
                              .SelectWhere((Instruction inst, [NotNullWhen(true)] out FieldInfo? fld) =>
                              {
                                  if (inst.Arg.Is(out fld) &&
                                      fld.DeclaringType == owner &&
                                      fld.FieldType == propertyInfo.PropertyType)
                                  {
                                      return true;
                                  }

                                  fld = null;
                                  return false;
                              })
                              .OrderBy(fld => Levenshtein.Calculate(fld!.Name, propertyInfo.Name))
                              .FirstOrDefault();
            }
        }
*/
        return field;
    }

}