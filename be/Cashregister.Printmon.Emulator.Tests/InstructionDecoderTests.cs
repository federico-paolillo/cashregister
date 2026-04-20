using System.Collections.Immutable;

using Cashregister.Printmon.Emulator.Problems;
using Cashregister.Printmon.Encoders;
using Cashregister.Printmon.Instructions;
using Cashregister.Printmon.Instructions.CodePage;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Cut;
using Cashregister.Printmon.Instructions.Feed;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Motion;
using Cashregister.Printmon.Instructions.Peripheral;

namespace Cashregister.Printmon.Emulator.Tests;

public sealed class InstructionDecoderTests
{
    private readonly InstructionDecoder _decoder = new();

    private static byte[] BuildBytes(Action<PrintProgramBuilder> configure)
    {
        var builder = new PrintProgramBuilder();
        configure(builder);
        return new BinaryEncoder().Encode(builder.Build()).Value!;
    }

    // Unwraps the result for happy-path tests
    private ImmutableArray<Instruction> Decode(byte[] bytes) =>
        _decoder.Decode(bytes).Value!.Instructions;

    // ---- Round-trip: empty program ----

    [Fact]
    public void Decode_EmptyProgram_ReturnsExpectedInstructions()
    {
        var bytes = BuildBytes(_ => { });

        var instructions = Decode(bytes);

        // Preamble: Init + SelectCodePage(OEM437) + ResetPrintMode
        // Postamble: LineFeed + CutAfter(1)
        Assert.Equal(5, instructions.Length);
        Assert.IsType<InitializeInstruction>(instructions[0]);
        var cp = Assert.IsType<SelectCodePageInstruction>(instructions[1]);
        Assert.Equal(CharacterCodePage.OEM437, cp.Page);
        Assert.IsType<ResetPrintModeInstruction>(instructions[2]);
        Assert.IsType<LineFeedInstruction>(instructions[3]);
        var cut = Assert.IsType<CutAfterInstruction>(instructions[4]);
        Assert.Equal(1, cut.Distance);
    }

    // ---- Text ----

    [Fact]
    public void Decode_Text_ProducesSingleTextInstruction()
    {
        var bytes = BuildBytes(b => b.Text("Hello"));

        var instructions = Decode(bytes);

        var text = Assert.IsType<TextInstruction>(instructions[3]);
        Assert.Equal("Hello", text.Text);
    }

    [Fact]
    public void Decode_ConsecutiveText_MergesIntoSingleTextInstruction()
    {
        // Two consecutive Text calls produce adjacent ASCII bytes — decoder merges them
        var bytes = BuildBytes(b => b.Text("AB").Text("CD"));

        var instructions = Decode(bytes);

        // "AB" and "CD" are adjacent ASCII bytes -> single TextInstruction("ABCD")
        var text = Assert.IsType<TextInstruction>(instructions[3]);
        Assert.Equal("ABCD", text.Text);
    }

    [Fact]
    public void Decode_LineFeed_ProducesLineFeedInstruction()
    {
        var bytes = BuildBytes(b => b.LineFeed());

        var instructions = Decode(bytes);

        Assert.IsType<LineFeedInstruction>(instructions[3]);
    }

    // ---- Formatting ----

    [Fact]
    public void Decode_BoldOn_ProducesEmphasizeInstruction_Enabled()
    {
        var bytes = BuildBytes(b => b.BoldOn());

        var instructions = Decode(bytes);

        var emp = Assert.IsType<EmphasizeInstruction>(instructions[3]);
        Assert.True(emp.Enabled);
    }

    [Fact]
    public void Decode_BoldOff_ProducesEmphasizeInstruction_Disabled()
    {
        var bytes = BuildBytes(b => b.BoldOff());

        var instructions = Decode(bytes);

        var emp = Assert.IsType<EmphasizeInstruction>(instructions[3]);
        Assert.False(emp.Enabled);
    }

    [Fact]
    public void Decode_DoubleStrikeOn_ProducesDoubleStrikeInstruction_Enabled()
    {
        var bytes = BuildBytes(b => b.DoubleStrikeOn());

        var instructions = Decode(bytes);

        var ds = Assert.IsType<DoubleStrikeInstruction>(instructions[3]);
        Assert.True(ds.Enabled);
    }

    [Fact]
    public void Decode_UnderlineOn_ProducesUnderlineInstruction_Enabled()
    {
        var bytes = BuildBytes(b => b.UnderlineOn(Thickness.Thin));

        var instructions = Decode(bytes);

        var u = Assert.IsType<UnderlineInstruction>(instructions[3]);
        Assert.True(u.Enabled);
        Assert.Equal(Thickness.Thin, u.Thickness);
    }

    [Fact]
    public void Decode_UnderlineThick_ProducesUnderlineInstruction_Thick()
    {
        var bytes = BuildBytes(b => b.UnderlineOn(Thickness.Thick));

        var instructions = Decode(bytes);

        var u = Assert.IsType<UnderlineInstruction>(instructions[3]);
        Assert.Equal(Thickness.Thick, u.Thickness);
    }

    [Fact]
    public void Decode_UnderlineOff_ProducesUnderlineInstruction_Disabled()
    {
        var bytes = BuildBytes(b => b.UnderlineOff());

        var instructions = Decode(bytes);

        var u = Assert.IsType<UnderlineInstruction>(instructions[3]);
        Assert.False(u.Enabled);
    }

    [Fact]
    public void Decode_SelectFontB_ProducesSelectFontInstruction_FontB()
    {
        var bytes = BuildBytes(b => b.Font(CharacterFont.B));

        var instructions = Decode(bytes);

        var sf = Assert.IsType<SelectFontInstruction>(instructions[3]);
        Assert.Equal(CharacterFont.B, sf.Font);
    }

    [Fact]
    public void Decode_RotateOn_ProducesRotationInstruction_Enabled()
    {
        var bytes = BuildBytes(b => b.RotateOn());

        var instructions = Decode(bytes);

        var r = Assert.IsType<RotationInstruction>(instructions[3]);
        Assert.True(r.Enabled);
    }

    [Fact]
    public void Decode_UpsideDownOn_ProducesUpsideDownInstruction_Enabled()
    {
        var bytes = BuildBytes(b => b.UpsideDownOn());

        var instructions = Decode(bytes);

        var ud = Assert.IsType<UpsideDownInstruction>(instructions[3]);
        Assert.True(ud.Enabled);
    }

    [Fact]
    public void Decode_InvertOn_ProducesReverseInstruction_Enabled()
    {
        var bytes = BuildBytes(b => b.InvertOn());

        var instructions = Decode(bytes);

        var rev = Assert.IsType<ReverseInstruction>(instructions[3]);
        Assert.True(rev.Enabled);
    }

    [Fact]
    public void Decode_FontSize_ProducesFontSizeInstruction_CorrectSize()
    {
        var bytes = BuildBytes(b => b.FontSize(2, 3));

        var instructions = Decode(bytes);

        var fs = Assert.IsType<FontSizeInstruction>(instructions[3]);
        // Builder packs: ((width-1) << 4) | (height-1) = (1 << 4) | 2 = 0x12
        Assert.Equal(0x12, fs.Size);
    }

    // ---- ESC ! disambiguation ----

    [Fact]
    public void Decode_PrintModeReset_ProducesResetPrintModeInstruction()
    {
        // ESC ! 0x00 -> ResetPrintModeInstruction
        var bytes = new byte[] { 0x1B, 0x21, 0x00 };

        var instructions = Decode(bytes);

        Assert.Single(instructions);
        Assert.IsType<ResetPrintModeInstruction>(instructions[0]);
    }

    [Fact]
    public void Decode_PrintModeNonZero_ProducesSelectPrintModeInstruction()
    {
        // ESC ! 0x08 (Emphasized) -> SelectPrintModeInstruction
        var bytes = new byte[] { 0x1B, 0x21, 0x08 };

        var instructions = Decode(bytes);

        Assert.Single(instructions);
        var spm = Assert.IsType<SelectPrintModeInstruction>(instructions[0]);
        Assert.False(spm.UseFontB);
        Assert.Equal(FormatMode.Emphasized, spm.Flags);
    }

    [Fact]
    public void Decode_PrintMode_FontBWithFlags_ProducesSelectPrintModeInstruction()
    {
        var bytes = BuildBytes(b => b.PrintMode(CharacterFont.B, FormatMode.Emphasized));

        var instructions = Decode(bytes);

        var spm = Assert.IsType<SelectPrintModeInstruction>(instructions[3]);
        Assert.True(spm.UseFontB);
        Assert.Equal(FormatMode.Emphasized, spm.Flags);
    }

    // ---- Layout ----

    [Fact]
    public void Decode_AlignCenter_ProducesJustifyInstruction_Center()
    {
        var bytes = BuildBytes(b => b.Align(Alignment.Center));

        var instructions = Decode(bytes);

        var j = Assert.IsType<JustifyInstruction>(instructions[3]);
        Assert.Equal(Alignment.Center, j.Alignment);
    }

    [Fact]
    public void Decode_LeftMargin_ProducesLeftMarginInstruction()
    {
        var bytes = BuildBytes(b => b.LeftMargin(100));

        var instructions = Decode(bytes);

        var lm = Assert.IsType<LeftMarginInstruction>(instructions[3]);
        Assert.Equal(100, lm.Margin);
    }

    [Fact]
    public void Decode_AbsolutePosition_ProducesAbsolutePositionInstruction()
    {
        var bytes = BuildBytes(b => b.MoveToColumn(200));

        var instructions = Decode(bytes);

        var ap = Assert.IsType<AbsolutePositionInstruction>(instructions[3]);
        Assert.Equal(200, ap.Position);
    }

    [Fact]
    public void Decode_PrintAreaWidth_ProducesPrintAreaWidthInstruction()
    {
        var bytes = BuildBytes(b => b.PrintWidth(400));

        var instructions = Decode(bytes);

        var pw = Assert.IsType<PrintAreaWidthInstruction>(instructions[3]);
        Assert.Equal(400, pw.Width);
    }

    // ---- Feed ----

    [Fact]
    public void Decode_FeedLines_ProducesFeedLinesInstruction()
    {
        var bytes = BuildBytes(b => b.FeedLines(3));

        var instructions = Decode(bytes);

        var fl = Assert.IsType<FeedLinesInstruction>(instructions[3]);
        Assert.Equal(3, fl.Lines);
    }

    [Fact]
    public void Decode_ResetLineSpacing_ProducesResetLineSpacingInstruction()
    {
        var bytes = BuildBytes(b => b.ResetLineSpacing());

        var instructions = Decode(bytes);

        Assert.IsType<ResetLineSpacingInstruction>(instructions[3]);
    }

    [Fact]
    public void Decode_SetLineSpacing_ProducesSetLineSpacingInstruction()
    {
        var bytes = BuildBytes(b => b.SetLineSpacing(5.0));

        var instructions = Decode(bytes);

        var sls = Assert.IsType<SetLineSpacingInstruction>(instructions[3]);
        Assert.Equal(40, sls.Spacing); // 5.0 / 0.125 = 40
    }

    // ---- Motion ----

    [Fact]
    public void Decode_HorizontalTab_ProducesHorizontalTabInstruction()
    {
        var bytes = BuildBytes(b => b.HorizontalTab());

        var instructions = Decode(bytes);

        Assert.IsType<HorizontalTabInstruction>(instructions[3]);
    }

    [Fact]
    public void Decode_SetHorizontalTabs_ProducesSetHorizontalTabsInstruction_WithPositions()
    {
        var bytes = BuildBytes(b => b.SetHorizontalTabs(8, 16, 24));

        var instructions = Decode(bytes);

        var tabs = Assert.IsType<SetHorizontalTabsInstruction>(instructions[3]);
        Assert.Equal(3, tabs.Positions.Length);
        Assert.Equal(8, tabs.Positions[0]);
        Assert.Equal(16, tabs.Positions[1]);
        Assert.Equal(24, tabs.Positions[2]);
    }

    [Fact]
    public void Decode_ClearHorizontalTabs_ProducesSetHorizontalTabsInstruction_Empty()
    {
        var bytes = BuildBytes(b => b.ClearHorizontalTabs());

        var instructions = Decode(bytes);

        var tabs = Assert.IsType<SetHorizontalTabsInstruction>(instructions[3]);
        Assert.Empty(tabs.Positions);
    }

    // ---- Cut ----

    [Fact]
    public void Decode_HalfCut_ProducesHalfCutInstruction()
    {
        var bytes = BuildBytes(b => b.HalfCut());

        var instructions = Decode(bytes);

        Assert.IsType<HalfCutInstruction>(instructions[3]);
    }

    [Fact]
    public void Decode_PartialCut_ProducesCutInstruction()
    {
        // builder.PartialCut() -> CutInstruction (GS V 1), not PartialCutInstruction
        var bytes = BuildBytes(b => b.PartialCut());

        var instructions = Decode(bytes);

        Assert.IsType<CutInstruction>(instructions[3]);
    }

    [Fact]
    public void Decode_FeedAndCut_ProducesCutAfterInstruction()
    {
        var bytes = BuildBytes(b => b.FeedAndCut(5));

        var instructions = Decode(bytes);

        var cut = Assert.IsType<CutAfterInstruction>(instructions[3]);
        Assert.Equal(5, cut.Distance);
    }

    // ---- GS V disambiguation ----

    [Fact]
    public void Decode_GsV01_ProducesCutInstruction()
    {
        var bytes = new byte[] { 0x1D, 0x56, 0x01 };

        var instructions = Decode(bytes);

        Assert.Single(instructions);
        Assert.IsType<CutInstruction>(instructions[0]);
    }

    [Fact]
    public void Decode_GsV42_ProducesCutAfterInstruction()
    {
        var bytes = new byte[] { 0x1D, 0x56, 0x42, 0x03 };

        var instructions = Decode(bytes);

        Assert.Single(instructions);
        var cut = Assert.IsType<CutAfterInstruction>(instructions[0]);
        Assert.Equal(3, cut.Distance);
    }

    // ---- CodePage ----

    [Fact]
    public void Decode_SelectCodePage_ProducesSelectCodePageInstruction()
    {
        var bytes = BuildBytes(b => b.SelectCodePage(CharacterCodePage.OEM850));

        var instructions = Decode(bytes);

        var cp = Assert.IsType<SelectCodePageInstruction>(instructions[3]);
        Assert.Equal(CharacterCodePage.OEM850, cp.Page);
    }

    // ---- Peripheral ----

    [Fact]
    public void Decode_OpenCashDrawer_ProducesGeneratePulseInstruction()
    {
        var bytes = BuildBytes(b => b.OpenCashDrawer());

        var instructions = Decode(bytes);

        var pulse = Assert.IsType<GeneratePulseInstruction>(instructions[3]);
        Assert.Equal(ConnectorPin.Pin2, pulse.Pin);
        Assert.Equal(25, pulse.OnTime);
        Assert.Equal(250, pulse.OffTime);
    }

    // ---- Error paths ----

    [Fact]
    public void Decode_UnknownEscByte_ReturnsUnrecognizedBytesProblem_WithContext()
    {
        var bytes = new byte[] { 0x1B, 0xFF };

        var result = _decoder.Decode(bytes);

        Assert.True(result.NotOk);
        var problem = Assert.IsType<UnrecognizedBytesProblem>(result.Error);
        Assert.Contains("0xFF", problem.Context, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Decode_TruncatedEscSequence_ReturnsTruncatedSequenceProblem()
    {
        var bytes = new byte[] { 0x1B }; // ESC with no following byte

        var result = _decoder.Decode(bytes);

        Assert.True(result.NotOk);
        Assert.IsType<TruncatedSequenceProblem>(result.Error);
    }

    [Fact]
    public void Decode_TruncatedEscWithSecondByte_ReturnsTruncatedSequenceProblem()
    {
        var bytes = new byte[] { 0x1B, 0x45 }; // ESC E with no n byte

        var result = _decoder.Decode(bytes);

        Assert.True(result.NotOk);
        Assert.IsType<TruncatedSequenceProblem>(result.Error);
    }

    [Fact]
    public void Decode_UnknownControlByte_ReturnsUnrecognizedBytesProblem_WithContext()
    {
        var bytes = new byte[] { 0xFF };

        var result = _decoder.Decode(bytes);

        Assert.True(result.NotOk);
        var problem = Assert.IsType<UnrecognizedBytesProblem>(result.Error);
        Assert.Contains("0xFF", problem.Context, StringComparison.OrdinalIgnoreCase);
    }
}