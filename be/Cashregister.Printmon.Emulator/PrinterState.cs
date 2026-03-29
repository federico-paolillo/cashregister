using System.Collections.Immutable;

using Cashregister.Printmon.Instructions.CodePage;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Printmon.Emulator;

public sealed record PrinterState
{
    // Formatting
    public bool Bold { get; init; }
    public Thickness Underline { get; init; }
    public bool DoubleStrike { get; init; }
    public CharacterFont Font { get; init; }
    public bool Rotation { get; init; }
    public bool UpsideDown { get; init; }
    public bool Reverse { get; init; }
    public byte WidthMultiplier { get; init; }
    public byte HeightMultiplier { get; init; }

    // Layout
    public Alignment Justification { get; init; }
    public ushort LeftMargin { get; init; }
    public ushort PrintAreaWidth { get; init; }
    public byte RightSpacing { get; init; }

    // Feed
    public byte LineSpacing { get; init; }

    // CodePage
    public CharacterCodePage CodePage { get; init; }

    // Motion
    public ImmutableArray<byte> TabPositions { get; init; }

    public static PrinterState Default { get; } = new()
    {
        Bold = false,
        Underline = Thickness.None,
        DoubleStrike = false,
        Font = CharacterFont.A,
        Rotation = false,
        UpsideDown = false,
        Reverse = false,
        WidthMultiplier = 1,
        HeightMultiplier = 1,
        Justification = Alignment.Left,
        LeftMargin = 0,
        PrintAreaWidth = 0,
        RightSpacing = 0,
        LineSpacing = 30,
        CodePage = CharacterCodePage.OEM437,
        TabPositions = [9, 17, 25, 33, 41, 49, 57]
    };
}
