using Cashregister.Printmon.Instructions;
using Cashregister.Printmon.Instructions.CodePage;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Cut;
using Cashregister.Printmon.Instructions.Feed;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Motion;
using Cashregister.Printmon.Instructions.Peripheral;

namespace Cashregister.Printmon;

/// <summary>
///     A builder pattern implementation that helps you build valid <see cref="PrintProgram" /><br />
///     This class is single-use. Once you build the <see cref="PrintProgram" /> the builder is frozen.
/// </summary>
public sealed class PrintProgramBuilder
{
    private readonly List<Instruction> instructions = [new InitializeInstruction(), new SelectCodePageInstruction(CharacterCodePage.OEM437), new ResetPrintModeInstruction()];
    private bool frozen;

    // Core

    public PrintProgramBuilder NoOp()
    {
        AddInstruction(new NoOpInstruction());

        return this;
    }

    public PrintProgramBuilder Text(string text)
    {
        AddInstruction(new TextInstruction(text));

        return this;
    }

    public PrintProgramBuilder LineFeed()
    {
        AddInstruction(new LineFeedInstruction());

        return this;
    }

    public PrintProgramBuilder PrintLine(string text)
    {
        return Text(text).LineFeed();
    }

    // Formatting

    public PrintProgramBuilder PrintMode(CharacterFont font, FormatMode formatMode)
    {
        AddInstruction(new SelectPrintModeInstruction(font == CharacterFont.B, formatMode));

        return this;
    }

    public PrintProgramBuilder UnderlineOn(Thickness thickness)
    {
        AddInstruction(new UnderlineInstruction(true, thickness));

        return this;
    }

    public PrintProgramBuilder UnderlineOff()
    {
        AddInstruction(new UnderlineInstruction(false, default));

        return this;
    }

    public PrintProgramBuilder BoldOn()
    {
        AddInstruction(new EmphasizeInstruction(true));

        return this;
    }

    public PrintProgramBuilder BoldOff()
    {
        AddInstruction(new EmphasizeInstruction(false));

        return this;
    }

    public PrintProgramBuilder DoubleStrikeOn()
    {
        AddInstruction(new DoubleStrikeInstruction(true));

        return this;
    }

    public PrintProgramBuilder DoubleStrikeOff()
    {
        AddInstruction(new DoubleStrikeInstruction(false));

        return this;
    }

    public PrintProgramBuilder Font(CharacterFont font)
    {
        AddInstruction(new SelectFontInstruction(font));

        return this;
    }

    public PrintProgramBuilder RotateOn()
    {
        AddInstruction(new RotationInstruction(true));

        return this;
    }

    public PrintProgramBuilder RotateOff()
    {
        AddInstruction(new RotationInstruction(false));

        return this;
    }

    public PrintProgramBuilder UpsideDownOn()
    {
        AddInstruction(new UpsideDownInstruction(true));

        return this;
    }

    public PrintProgramBuilder UpsideDownOff()
    {
        AddInstruction(new UpsideDownInstruction(false));

        return this;
    }

    public PrintProgramBuilder InvertOn()
    {
        AddInstruction(new ReverseInstruction(true));

        return this;
    }

    public PrintProgramBuilder InvertOff()
    {
        AddInstruction(new ReverseInstruction(false));

        return this;
    }

    public PrintProgramBuilder FontSize(int widthMultiplier, int heightMultiplier)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(widthMultiplier, 1, nameof(widthMultiplier));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(widthMultiplier, 8, nameof(widthMultiplier));
        ArgumentOutOfRangeException.ThrowIfLessThan(heightMultiplier, 1, nameof(heightMultiplier));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(heightMultiplier, 8, nameof(heightMultiplier));

        var size = (byte)(((widthMultiplier - 1) << 4) | (heightMultiplier - 1));

        AddInstruction(new FontSizeInstruction(size));

        return this;
    }

    public PrintProgramBuilder FontSize(int multiplier)
    {
        return FontSize(multiplier, multiplier);
    }

    // Layout

    public PrintProgramBuilder Align(Alignment alignment)
    {
        AddInstruction(new JustifyInstruction(alignment));

        return this;
    }

    public PrintProgramBuilder MoveToColumn(ushort dots)
    {
        AddInstruction(new AbsolutePositionInstruction(dots));

        return this;
    }

    public PrintProgramBuilder MoveBy(ushort dots)
    {
        AddInstruction(new RelativePositionInstruction(dots));

        return this;
    }

    public PrintProgramBuilder LeftMargin(ushort dots)
    {
        AddInstruction(new LeftMarginInstruction(dots));

        return this;
    }

    public PrintProgramBuilder SetCharacterSpacing(double millimeters)
    {
        var units = ConvertMillimetersToUnits(millimeters);

        AddInstruction(new RightSpacingInstruction(units));

        return this;
    }

    public PrintProgramBuilder PrintWidth(ushort dots)
    {
        AddInstruction(new PrintAreaWidthInstruction(dots));

        return this;
    }

    // Feed

    public PrintProgramBuilder ResetLineSpacing()
    {
        AddInstruction(new ResetLineSpacingInstruction());

        return this;
    }

    public PrintProgramBuilder SetLineSpacing(double millimeters)
    {
        var units = ConvertMillimetersToUnits(millimeters);

        AddInstruction(new SetLineSpacingInstruction(units));

        return this;
    }

    public PrintProgramBuilder FeedLines(byte lines)
    {
        AddInstruction(new FeedLinesInstruction(lines));

        return this;
    }

    public PrintProgramBuilder FeedPaper(double millimeters)
    {
        var units = ConvertMillimetersToUnits(millimeters);

        AddInstruction(new FeedPaperInstruction(units));

        return this;
    }

    // Motion

    public PrintProgramBuilder HorizontalTab()
    {
        AddInstruction(new HorizontalTabInstruction());

        return this;
    }

    public PrintProgramBuilder SetHorizontalTabs(params byte[] positions)
    {
        AddInstruction(new SetHorizontalTabsInstruction([.. positions]));

        return this;
    }

    public PrintProgramBuilder ClearHorizontalTabs()
    {
        AddInstruction(new SetHorizontalTabsInstruction([]));

        return this;
    }

    // Cut

    public PrintProgramBuilder FeedAndCut(byte lines)
    {
        AddInstruction(new CutAfterInstruction(lines));

        return this;
    }

    public PrintProgramBuilder HalfCut()
    {
        AddInstruction(new HalfCutInstruction());

        return this;
    }

    public PrintProgramBuilder PartialCut()
    {
        AddInstruction(new CutInstruction());

        return this;
    }

    // CodePage

    public PrintProgramBuilder SelectCodePage(CharacterCodePage page)
    {
        AddInstruction(new SelectCodePageInstruction(page));

        return this;
    }

    // Peripheral

    public PrintProgramBuilder KickDrawer(ConnectorPin pin, TimeSpan onDuration, TimeSpan offDuration)
    {
        var onTime = ConvertToKickDrawerUnit(onDuration, nameof(onDuration));
        var offTime = ConvertToKickDrawerUnit(offDuration, nameof(offDuration));

        AddInstruction(new GeneratePulseInstruction(pin, onTime, offTime));

        return this;
    }

    public PrintProgramBuilder OpenCashDrawer()
    {
        AddInstruction(new GeneratePulseInstruction(ConnectorPin.Pin2, 25, 250));

        return this;
    }

    public PrintProgramBuilder RealTimePulse(ConnectorPin pin, TimeSpan duration)
    {
        var units = (byte)(duration.TotalMilliseconds / 100);

        if (units is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be between 100ms and 800ms.");
        }

        AddInstruction(new RealTimePulseInstruction(pin, units));

        return this;
    }

    private void AddInstruction(Instruction instruction)
    {
        ArgumentNullException.ThrowIfNull(instruction);

        if (frozen)
        {
            throw new InvalidOperationException("This builder has already emitted its program");
        }

        instructions.Add(instruction);
    }

    public PrintProgram Build()
    {
        AddInstruction(new LineFeedInstruction());
        AddInstruction(new CutAfterInstruction(1));

        frozen = true;

        return new PrintProgram([.. instructions]);
    }

    private static byte ConvertMillimetersToUnits(double millimeters)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(millimeters, nameof(millimeters));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(millimeters, 31.875, nameof(millimeters));

        return (byte)(millimeters / 0.125);
    }

    private static byte ConvertToKickDrawerUnit(TimeSpan duration, string paramName)
    {
        var units = (byte)(duration.TotalMilliseconds / 2);

        if (units < 1)
        {
            throw new ArgumentOutOfRangeException(paramName, duration, "Duration must be at least 2ms.");
        }

        return units;
    }
}