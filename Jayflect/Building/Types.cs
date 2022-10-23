namespace Jayflect.Building;

/// <summary>
/// Represents a placeholder instance <see cref="Type"/> for accessing <see langword="static"/> methods
/// </summary>
public struct NoInstance
{
    private static NoInstance _instance;

    /// <summary>
    /// Gets a <see langword="ref"/> to an instance of <see cref="NoInstance"/> for use in accessing <see langword="static"/> methods
    /// </summary>
    public static ref NoInstance Ref
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _instance;
    }
}