namespace Jayflect.Fulfilling;

public interface ICloneable<T> : ICloneable
    where T : ICloneable<T>
{
    object ICloneable.Clone() => (object)Clone();

    new T Clone();
}