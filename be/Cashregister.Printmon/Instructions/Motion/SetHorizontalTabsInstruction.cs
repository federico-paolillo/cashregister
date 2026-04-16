using System.Collections.Immutable;

namespace Cashregister.Printmon.Instructions.Motion;

public sealed record SetHorizontalTabsInstruction : Instruction
{
    public SetHorizontalTabsInstruction(ImmutableArray<byte> positions)
    {
        if (positions.Length > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(positions), "Tab positions must have at most 32 entries.");
        }

        for (var i = 0; i < positions.Length; i++)
        {
            if (positions[i] == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(positions), "Tab position values must be between 1 and 255.");
            }

            if (i > 0 && positions[i] <= positions[i - 1])
            {
                throw new ArgumentOutOfRangeException(nameof(positions), "Tab positions must be in strictly ascending order.");
            }
        }

        Positions = positions;
    }

    public ImmutableArray<byte> Positions { get; }
}