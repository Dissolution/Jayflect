namespace Jay.Dumping;

public readonly ref struct DumpFormat
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DumpFormat(ReadOnlySpan<char> format) => new DumpFormat(format);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DumpFormat(string? format) => new DumpFormat(format);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<char>(DumpFormat format) => format._formatSpan;

    public static bool operator ==(DumpFormat left, DumpFormat right) => left.Equals(right);
    public static bool operator !=(DumpFormat left, DumpFormat right) => !left.Equals(right);

    public static bool operator >(DumpFormat left, DumpFormat right) => left.CompareTo(right) > 0;
    public static bool operator <(DumpFormat left, DumpFormat right) => left.CompareTo(right) < 0;

    public static bool operator >=(DumpFormat left, DumpFormat right) => left.CompareTo(right) >= 0;
    public static bool operator <=(DumpFormat left, DumpFormat right) => left.CompareTo(right) <= 0;

    public static DumpFormat None
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new DumpFormat();
    }

    public static DumpFormat Custom(ReadOnlySpan<char> format) => new DumpFormat(format);

    public static DumpFormat WithType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new DumpFormat("V");
    }
   

    private static int GetCompareValue(DumpFormat format)
    {
        // None/Default
        if (format.IsNone) return 0;
        
        // Defined?
        if (format.IsWithType) return 2;
        
        //if (format.IsCustom)
        return 1;                        
    }
    
    private readonly ReadOnlySpan<char> _formatSpan;

    public bool IsNone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _formatSpan.Length == 0;
    }
    
    public bool IsWithType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _formatSpan == "V";
    }

    public bool IsCustom => !IsNone && !IsWithType;

    private DumpFormat(ReadOnlySpan<char> formatSpan)
    {
        _formatSpan = formatSpan;
    }

    public string? GetCustomFormatString()
    {
        var format = _formatSpan;
        if (format.Length > 0 && format != "V")
        {
            return new string(format);
        }
        return null;
    }
    
    public bool Equals(DumpFormat dumpFormat) => _formatSpan.SequenceEqual(dumpFormat._formatSpan);

    public int CompareTo(DumpFormat dumpFormat) => GetCompareValue(this).CompareTo(GetCompareValue(dumpFormat));
    
    public override bool Equals(object? obj) => false;
    public override int GetHashCode() => 0;
    public override string ToString() => new string(_formatSpan);
}