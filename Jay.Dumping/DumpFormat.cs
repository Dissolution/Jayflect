namespace Jay.Dumping;

public readonly ref struct DumpFormat
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DumpFormat(string? format) => new DumpFormat(format);

    public static bool operator ==(DumpFormat left, DumpFormat right) => left.Equals(right);
    public static bool operator !=(DumpFormat left, DumpFormat right) => !left.Equals(right);

    public static bool operator >(DumpFormat left, DumpFormat right) => GetCValue(left) > GetCValue(right);
    public static bool operator <(DumpFormat left, DumpFormat right) => GetCValue(left) < GetCValue(right);
    
    public static bool operator >=(DumpFormat left, DumpFormat right) => GetCValue(left) >= GetCValue(right);
    public static bool operator <=(DumpFormat left, DumpFormat right) => GetCValue(left) <= GetCValue(right);

    public static DumpFormat None
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new DumpFormat();
    }

    public static DumpFormat View
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new DumpFormat("V");
    }

    public static DumpFormat Inspect
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new DumpFormat("I");
    }

    public static DumpFormat All
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new DumpFormat("A");
    }

    private static int GetCValue(DumpFormat format)
    {
        var span = format._formatSpan;
        if (span.Length == 0) return 0; // None
        if (span.Length > 1) return 1;  // Custom
        char ch = span[0];
        if (ch == 'V') return 2;        // View
        if (ch == 'I') return 3;        // Inspect
        if (ch == 'A') return 4;        // All
        // Fallback to Custom
        return 1;                        
    }
    
    private readonly ReadOnlySpan<char> _formatSpan;

    public bool IsNone
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _formatSpan.Length == 0;
    }
    
    public bool IsCustom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _formatSpan.Length > 0 && _formatSpan[0] is not 'V' or 'I' or 'A';
    }

    public bool IsView
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _formatSpan.Length > 0 && _formatSpan[0] == 'V';
    }
    
    public bool IsInspect
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _formatSpan.Length > 0 && _formatSpan[0] == 'I';
    }
    
    public bool IsAll
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _formatSpan.Length > 0 && _formatSpan[0] == 'A';
    }

    private DumpFormat(ReadOnlySpan<char> formatSpan)
    {
        _formatSpan = formatSpan;
    }

    public string? GetCustomFormatString()
    {
        var format = _formatSpan;
        if (format.Length > 0 && format[0] is not ('V' or 'I' or 'A'))
        {
            return new string(format);
        }
        return null;
    }
    
    public bool Equals(DumpFormat dumpFormat) => _formatSpan.SequenceEqual(dumpFormat._formatSpan);

    public int CompareTo(DumpFormat dumpFormat) => GetCValue(this).CompareTo(GetCValue(dumpFormat));
    
    public override bool Equals(object? obj) => false;
    public override int GetHashCode() => 0;
    public override string ToString() => new string(_formatSpan);
}