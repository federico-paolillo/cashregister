using System.Collections.Immutable;

using Cashregister.Factories;
using Cashregister.Printmon.Emulator.Problems;
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
    Result<Printer> Execute(Printer printer, Instruction instruction);
}

public sealed class InstructionExecutor : IInstructionExecutor
{
    public Result<Printer> Execute(Printer printer, Instruction instruction)
    {
        ArgumentNullException.ThrowIfNull(printer);
        ArgumentNullException.ThrowIfNull(instruction);

        return instruction switch
        {
            // Core
            NoOpInstruction =>
                Result.Ok(printer),

            InitializeInstruction =>
                Result.Ok(printer with { State = PrinterState.Default }),

            TextInstruction text =>
                Result.Ok(Emit(printer, new TextSpan(text.Text, StyleFrom(printer.State)))),

            LineFeedInstruction =>
                Result.Ok(Emit(printer, new LineBreak())),

            // Formatting
            ResetPrintModeInstruction =>
                Result.Ok(printer with
                {
                    State = printer.State with
                    {
                        Bold = false,
                        Underline = Thickness.None,
                        Font = CharacterFont.A,
                        WidthMultiplier = 1,
                        HeightMultiplier = 1
                    }
                }),

            SelectPrintModeInstruction spm =>
                Result.Ok(printer with
                {
                    State = printer.State with
                    {
                        Font = spm.UseFontB ? CharacterFont.B : CharacterFont.A,
                        Bold = spm.Flags.HasFlag(FormatMode.Emphasized),
                        WidthMultiplier = (byte)(spm.Flags.HasFlag(FormatMode.DoubleWidth) ? 2 : 1),
                        HeightMultiplier = (byte)(spm.Flags.HasFlag(FormatMode.DoubleHeight) ? 2 : 1),
                        Underline = spm.Flags.HasFlag(FormatMode.Underline) ? Thickness.Thin : Thickness.None
                    }
                }),

            UnderlineInstruction u =>
                Result.Ok(printer with { State = printer.State with { Underline = u.Enabled ? u.Thickness : Thickness.None } }),

            EmphasizeInstruction e =>
                Result.Ok(printer with { State = printer.State with { Bold = e.Enabled } }),

            DoubleStrikeInstruction ds =>
                Result.Ok(printer with { State = printer.State with { DoubleStrike = ds.Enabled } }),

            SelectFontInstruction sf =>
                Result.Ok(printer with { State = printer.State with { Font = sf.Font } }),

            FontSizeInstruction fs =>
                Result.Ok(printer with
                {
                    State = printer.State with
                    {
                        WidthMultiplier = (byte)((fs.Size >> 4) + 1),
                        HeightMultiplier = (byte)((fs.Size & 0x0F) + 1)
                    }
                }),

            RotationInstruction r =>
                Result.Ok(printer with { State = printer.State with { Rotation = r.Enabled } }),

            UpsideDownInstruction ud =>
                Result.Ok(printer with { State = printer.State with { UpsideDown = ud.Enabled } }),

            ReverseInstruction rev =>
                Result.Ok(printer with { State = printer.State with { Reverse = rev.Enabled } }),

            // Layout
            JustifyInstruction j =>
                Result.Ok(printer with { State = printer.State with { Justification = j.Alignment } }),

            AbsolutePositionInstruction =>
                Result.Ok(printer),

            RelativePositionInstruction =>
                Result.Ok(printer),

            LeftMarginInstruction lm =>
                Result.Ok(printer with { State = printer.State with { LeftMargin = lm.Margin } }),

            RightSpacingInstruction rs =>
                Result.Ok(printer with { State = printer.State with { RightSpacing = rs.Spacing } }),

            PrintAreaWidthInstruction pw =>
                Result.Ok(printer with { State = printer.State with { PrintAreaWidth = pw.Width } }),

            // Feed
            ResetLineSpacingInstruction =>
                Result.Ok(printer with { State = printer.State with { LineSpacing = 30 } }),

            SetLineSpacingInstruction sls =>
                Result.Ok(printer with { State = printer.State with { LineSpacing = sls.Spacing } }),

            FeedLinesInstruction fl =>
                Result.Ok(Emit(printer, new FeedLines(fl.Lines))),

            FeedPaperInstruction =>
                Result.Ok(Emit(printer, new FeedLines(1))),

            // Motion
            HorizontalTabInstruction =>
                Result.Ok(Emit(printer, new TextSpan("\t", StyleFrom(printer.State)))),

            SetHorizontalTabsInstruction st =>
                Result.Ok(printer with { State = printer.State with { TabPositions = st.Positions } }),

            // Cut
            PartialCutInstruction =>
                Result.Ok(Emit(printer, new HorizontalRule())),

            CutAfterInstruction =>
                Result.Ok(Emit(printer, new HorizontalRule())),

            HalfCutInstruction =>
                Result.Ok(Emit(printer, new HorizontalRule())),

            CutInstruction =>
                Result.Ok(Emit(printer, new HorizontalRule())),

            // CodePage
            SelectCodePageInstruction cp =>
                Result.Ok(printer with { State = printer.State with { CodePage = cp.Page } }),

            // Peripheral
            GeneratePulseInstruction =>
                Result.Ok(printer),

            RealTimePulseInstruction =>
                Result.Ok(printer),

            _ => Result.Error<Printer>(new UnsupportedInstructionProblem(instruction.GetType().Name))
        };
    }

    private static Printer Emit(Printer printer, IDocumentElement element) =>
        printer with { Receipt = new Receipt(printer.Receipt.Elements.Add(element)) };

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
