using System.Collections.Immutable;

using Cashregister.Printmon.Instructions;
using Cashregister.Printmon.Instructions.CodePage;
using Cashregister.Printmon.Instructions.Core;
using Cashregister.Printmon.Instructions.Cut;
using Cashregister.Printmon.Instructions.Feed;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;
using Cashregister.Printmon.Instructions.Motion;
using Cashregister.Printmon.Instructions.Peripheral;

namespace Cashregister.Printmon.Emulator;

public interface IInstructionExecutor
{
    PrinterDocument Execute(PrinterState state, Instruction instruction);
}

public sealed class InstructionExecutor : IInstructionExecutor
{
    public PrinterDocument Execute(PrinterState state, Instruction instruction)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(instruction);

        return instruction switch
        {
            // Core
            NoOpInstruction =>
                new PrinterDocument(state, []),

            InitializeInstruction =>
                new PrinterDocument(PrinterState.Default, []),

            TextInstruction text =>
                new PrinterDocument(state, [new TextSpan(text.Text, StyleFrom(state))]),

            LineFeedInstruction =>
                new PrinterDocument(state, [new LineBreak()]),

            // Formatting
            ResetPrintModeInstruction =>
                new PrinterDocument(state with
                {
                    Bold = false,
                    Underline = Thickness.None,
                    Font = CharacterFont.A,
                    WidthMultiplier = 1,
                    HeightMultiplier = 1
                }, []),

            SelectPrintModeInstruction spm =>
                new PrinterDocument(state with
                {
                    Font = spm.UseFontB ? CharacterFont.B : CharacterFont.A,
                    Bold = spm.Flags.HasFlag(FormatMode.Emphasized),
                    WidthMultiplier = (byte)(spm.Flags.HasFlag(FormatMode.DoubleWidth) ? 2 : 1),
                    HeightMultiplier = (byte)(spm.Flags.HasFlag(FormatMode.DoubleHeight) ? 2 : 1),
                    Underline = spm.Flags.HasFlag(FormatMode.Underline) ? Thickness.Thin : Thickness.None
                }, []),

            UnderlineInstruction u =>
                new PrinterDocument(state with
                {
                    Underline = u.Enabled ? u.Thickness : Thickness.None
                }, []),

            EmphasizeInstruction e =>
                new PrinterDocument(state with { Bold = e.Enabled }, []),

            DoubleStrikeInstruction ds =>
                new PrinterDocument(state with { DoubleStrike = ds.Enabled }, []),

            SelectFontInstruction sf =>
                new PrinterDocument(state with { Font = sf.Font }, []),

            FontSizeInstruction fs =>
                new PrinterDocument(state with
                {
                    WidthMultiplier = (byte)((fs.Size >> 4) + 1),
                    HeightMultiplier = (byte)((fs.Size & 0x0F) + 1)
                }, []),

            RotationInstruction r =>
                new PrinterDocument(state with { Rotation = r.Enabled }, []),

            UpsideDownInstruction ud =>
                new PrinterDocument(state with { UpsideDown = ud.Enabled }, []),

            ReverseInstruction rev =>
                new PrinterDocument(state with { Reverse = rev.Enabled }, []),

            // Layout
            JustifyInstruction j =>
                new PrinterDocument(state with { Justification = j.Alignment }, []),

            AbsolutePositionInstruction =>
                new PrinterDocument(state, []),

            RelativePositionInstruction =>
                new PrinterDocument(state, []),

            LeftMarginInstruction lm =>
                new PrinterDocument(state with { LeftMargin = lm.Margin }, []),

            RightSpacingInstruction rs =>
                new PrinterDocument(state with { RightSpacing = rs.Spacing }, []),

            PrintAreaWidthInstruction pw =>
                new PrinterDocument(state with { PrintAreaWidth = pw.Width }, []),

            // Feed
            ResetLineSpacingInstruction =>
                new PrinterDocument(state with { LineSpacing = 30 }, []),

            SetLineSpacingInstruction sls =>
                new PrinterDocument(state with { LineSpacing = sls.Spacing }, []),

            FeedLinesInstruction fl =>
                new PrinterDocument(state, [new FeedLines(fl.Lines)]),

            FeedPaperInstruction =>
                new PrinterDocument(state, [new FeedLines(1)]),

            // Motion
            HorizontalTabInstruction =>
                new PrinterDocument(state, [new TextSpan("\t", StyleFrom(state))]),

            SetHorizontalTabsInstruction st =>
                new PrinterDocument(state with { TabPositions = st.Positions }, []),

            // Cut
            PartialCutInstruction =>
                new PrinterDocument(state, [new HorizontalRule()]),

            CutAfterInstruction =>
                new PrinterDocument(state, [new HorizontalRule()]),

            HalfCutInstruction =>
                new PrinterDocument(state, [new HorizontalRule()]),

            CutInstruction =>
                new PrinterDocument(state, [new HorizontalRule()]),

            // CodePage
            SelectCodePageInstruction cp =>
                new PrinterDocument(state with { CodePage = cp.Page }, []),

            // Peripheral
            GeneratePulseInstruction =>
                new PrinterDocument(state, []),

            RealTimePulseInstruction =>
                new PrinterDocument(state, []),

            _ => throw new NotSupportedException(
                $"Instruction {instruction.GetType().Name} is not supported by this executor.")
        };
    }

    private static TextStyle StyleFrom(PrinterState s) => new(
        Bold: s.Bold,
        Underline: s.Underline,
        DoubleStrike: s.DoubleStrike,
        Font: s.Font,
        Rotation: s.Rotation,
        UpsideDown: s.UpsideDown,
        Reverse: s.Reverse,
        WidthMultiplier: s.WidthMultiplier,
        HeightMultiplier: s.HeightMultiplier,
        Justification: s.Justification);
}
