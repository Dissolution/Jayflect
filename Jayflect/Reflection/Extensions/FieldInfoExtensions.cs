using System.Reflection;
using Jayflect;

namespace Jay.Reflection;

public static class FieldInfoExtensions
{
    public static Visibility Visibility(this FieldInfo? fieldInfo)
    {
        Visibility visibility = Jayflect.Visibility.None;
        if (fieldInfo is null)
            return visibility;
        if (fieldInfo.IsPrivate)
            visibility |= Jayflect.Visibility.Private;
        if (fieldInfo.IsFamily)
            visibility |= Jayflect.Visibility.Protected;
        if (fieldInfo.IsAssembly)
            visibility |= Jayflect.Visibility.Internal;
        if (fieldInfo.IsPublic)
            visibility |= Jayflect.Visibility.Public;
        return visibility;
    }
}