using Jayflect.Building.Emission.Instructions;

namespace Jayflect.Building.Emission;

public interface IFluentIL<TSelf>
    where TSelf : IFluentIL<TSelf>
{
    /// <summary>
    /// Gets the stream of <see cref="InstructionLine"/>s emitted thus far
    /// </summary>
    InstructionStream Instructions { get; }
}