namespace EscPosEmulator;

/// <summary>
/// Represents the current formatting state while interpreting commands.
/// </summary>
internal sealed class FormattingState
{
    public bool Bold { get; set; }
    public bool Underline { get; set; }
    public bool DoubleWidth { get; set; }
    public bool DoubleHeight { get; set; }
    public bool UpsideDown { get; set; }
    public bool Invert { get; set; }
    public int Font { get; set; }
    public int LineSpacing { get; set; } = DefaultLineSpacing;
    public int AbsolutePosition { get; set; }
    public int RelativePosition { get; set; }
    public int WidthMultiplier { get; set; } = 1;
    public int HeightMultiplier { get; set; } = 1;
    public Justification Justification { get; set; } = Justification.Left;
    public int LeftMargin { get; set; }
    public int PrintAreaWidth { get; set; }
    public int CharacterSpacing { get; set; }
    public int Rotation { get; set; }

    public const int DefaultLineSpacing = 30;

    public void Reset()
    {
        Bold = false;
        Underline = false;
        DoubleWidth = false;
        DoubleHeight = false;
        UpsideDown = false;
        Invert = false;
        Font = 0;
        LineSpacing = DefaultLineSpacing;
        AbsolutePosition = 0;
        RelativePosition = 0;
        WidthMultiplier = 1;
        HeightMultiplier = 1;
        Justification = Justification.Left;
        LeftMargin = 0;
        PrintAreaWidth = 0;
        CharacterSpacing = 0;
        Rotation = 0;
    }
}

internal enum Justification : byte
{
    Left = 0,
    Center = 1,
    Right = 2
}
