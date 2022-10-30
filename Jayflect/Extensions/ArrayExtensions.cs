namespace Jayflect.Extensions;

public static class ArrayExtensions
{
    public static Type[] ToTypeArray(this object?[]? objectArray)
    {
        if (objectArray is null) return Type.EmptyTypes;
        return Array.ConvertAll(objectArray, obj => obj?.GetType() ?? typeof(object));
    }
}