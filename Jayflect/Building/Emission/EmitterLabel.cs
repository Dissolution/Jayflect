using Jay.Dumping;
using Jay.Dumping.Extensions;

namespace Jayflect.Building.Emission;

public sealed class EmitterLabel : IEquatable<EmitterLabel>, IEquatable<Label>, IDumpable
{
    public static implicit operator Label(EmitterLabel label) => label._label;

    public static bool operator ==(EmitterLabel left, EmitterLabel right) => left.Equals(right);
    public static bool operator !=(EmitterLabel left, EmitterLabel right) => !left.Equals(right);
    public static bool operator ==(EmitterLabel left, Label right) => left._label == right;
    public static bool operator !=(EmitterLabel left, Label right) => left._label != right;
    
    private Label _label;
    private string _name;

    public Label Label
    {
        get => _label;
        internal set => _label = value;
    }

    public string Name
    {
        get => _name;
        internal set => _name = value;
    }
    public int Value => _label.GetHashCode();
    public bool IsShortForm => _label.IsShortForm();

    public EmitterLabel(Label label, string? name = null)
    {
        _label = label;
        if (string.IsNullOrWhiteSpace(name))
        {
            _name = $"Label{label.GetHashCode()}";
        }
        else
        {
            _name = name;
        }
        
    }

    public bool Equals(EmitterLabel? emitterLabel)
    {
        return emitterLabel is not null &&
               _label == emitterLabel._label &&
               string.Equals(_name, emitterLabel._name);
    }

    public bool Equals(Label label)
    {
        return _label == label;
    }

    public override bool Equals(object? obj)
    {
        if (obj is EmitterLabel emitterLabel) return Equals(emitterLabel);
        if (obj is Label label) return Equals(label);
        return false;
    }
    public override int GetHashCode()
    {
        return Value;
    }

    public void DumpTo(ref DefaultInterpolatedStringHandler stringHandler, DumpFormat dumpFormat = default)
    {
        // Mark (where the label is)
        if (dumpFormat == "M")
        {
            stringHandler.Write(Name);
            stringHandler.Write(':');
        }
        // Declare (when it is declared
        else if (dumpFormat == "D")
        {
            stringHandler.Write("declare :");
            stringHandler.Write(Name);
            stringHandler.Write(":");
        }
        // Use
        else
        {
            stringHandler.Write(Name);
        }
    }

    public override string ToString()
    {
        return Name;
    }
}