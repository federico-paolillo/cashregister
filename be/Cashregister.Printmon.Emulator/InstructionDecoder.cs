using System.Collections.Immutable;
using System.Text;

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

public interface IInstructionDecoder
{
    Result<PrintProgram> Decode(ReadOnlyMemory<byte> data);
}

public sealed class InstructionDecoder : IInstructionDecoder
{
    public Result<PrintProgram> Decode(ReadOnlyMemory<byte> data)
    {
        var span = data.Span;
        var instructions = ImmutableArray.CreateBuilder<Instruction>();
        var i = 0;

        while (i < span.Length)
        {
            switch (span[i])
            {
                case 0x0A: // LF
                    instructions.Add(new LineFeedInstruction());
                    i++;
                    break;

                case 0x09: // HT
                    instructions.Add(new HorizontalTabInstruction());
                    i++;
                    break;

                case 0x10: // DLE
                    // DLE DC4 0x01 m t -> RealTimePulseInstruction
                    if (RequireBytes(span, i, 5) is { } truncDle)
                        return Result.Error<PrintProgram>(truncDle);
                    if (span[i + 1] != 0x14 || span[i + 2] != 0x01)
                        return Result.Error<PrintProgram>(new UnrecognizedBytesProblem(
                            i, $"0x{span[i]:X2} 0x{span[i + 1]:X2} 0x{span[i + 2]:X2}"));
                    instructions.Add(new RealTimePulseInstruction((ConnectorPin)span[i + 3], span[i + 4]));
                    i += 5;
                    break;

                case 0x1B: // ESC
                    if (RequireBytes(span, i, 2) is { } truncEsc)
                        return Result.Error<PrintProgram>(truncEsc);
                    switch (span[i + 1])
                    {
                        case 0x40: // ESC @ -- Initialize
                            instructions.Add(new InitializeInstruction());
                            i += 2;
                            break;

                        case 0x21: // ESC ! n -- print mode / reset print mode
                            if (RequireBytes(span, i, 3) is { } t) return Result.Error<PrintProgram>(t);
                            var printModeN = span[i + 2];
                            instructions.Add(printModeN == 0
                                ? new ResetPrintModeInstruction()
                                : new SelectPrintModeInstruction((printModeN & 0x01) != 0, (FormatMode)(printModeN & ~0x01)));
                            i += 3;
                            break;

                        case 0x20: // ESC SP n -- right spacing
                            if (RequireBytes(span, i, 3) is { } t2) return Result.Error<PrintProgram>(t2);
                            instructions.Add(new RightSpacingInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x24: // ESC $ nL nH -- absolute position
                            if (RequireBytes(span, i, 4) is { } t3) return Result.Error<PrintProgram>(t3);
                            instructions.Add(new AbsolutePositionInstruction((ushort)(span[i + 2] | (span[i + 3] << 8))));
                            i += 4;
                            break;

                        case 0x2D: // ESC - n -- underline
                            if (RequireBytes(span, i, 3) is { } t4) return Result.Error<PrintProgram>(t4);
                            var underlineN = span[i + 2];
                            instructions.Add(new UnderlineInstruction(underlineN != 0, (Thickness)underlineN));
                            i += 3;
                            break;

                        case 0x32: // ESC 2 -- reset line spacing
                            instructions.Add(new ResetLineSpacingInstruction());
                            i += 2;
                            break;

                        case 0x33: // ESC 3 n -- set line spacing
                            if (RequireBytes(span, i, 3) is { } t5) return Result.Error<PrintProgram>(t5);
                            instructions.Add(new SetLineSpacingInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x44: // ESC D n1..nk NUL -- set horizontal tabs
                            i += 2;
                            var tabPositions = ImmutableArray.CreateBuilder<byte>();
                            while (i < span.Length && span[i] != 0x00)
                            {
                                tabPositions.Add(span[i]);
                                i++;
                            }
                            if (i >= span.Length)
                                return Result.Error<PrintProgram>(new TruncatedSequenceProblem(i, 1, 0));
                            i++; // consume NUL
                            instructions.Add(new SetHorizontalTabsInstruction(tabPositions.ToImmutable()));
                            break;

                        case 0x45: // ESC E n -- emphasize
                            if (RequireBytes(span, i, 3) is { } t6) return Result.Error<PrintProgram>(t6);
                            instructions.Add(new EmphasizeInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        case 0x47: // ESC G n -- double-strike
                            if (RequireBytes(span, i, 3) is { } t7) return Result.Error<PrintProgram>(t7);
                            instructions.Add(new DoubleStrikeInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        case 0x4A: // ESC J n -- feed paper
                            if (RequireBytes(span, i, 3) is { } t8) return Result.Error<PrintProgram>(t8);
                            instructions.Add(new FeedPaperInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x4D: // ESC M n -- select font
                            if (RequireBytes(span, i, 3) is { } t9) return Result.Error<PrintProgram>(t9);
                            instructions.Add(new SelectFontInstruction((CharacterFont)span[i + 2]));
                            i += 3;
                            break;

                        case 0x56: // ESC V n -- rotation
                            if (RequireBytes(span, i, 3) is { } t10) return Result.Error<PrintProgram>(t10);
                            instructions.Add(new RotationInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        case 0x5C: // ESC \ nL nH -- relative position
                            if (RequireBytes(span, i, 4) is { } t11) return Result.Error<PrintProgram>(t11);
                            instructions.Add(new RelativePositionInstruction((ushort)(span[i + 2] | (span[i + 3] << 8))));
                            i += 4;
                            break;

                        case 0x61: // ESC a n -- justification
                            if (RequireBytes(span, i, 3) is { } t12) return Result.Error<PrintProgram>(t12);
                            instructions.Add(new JustifyInstruction((Alignment)span[i + 2]));
                            i += 3;
                            break;

                        case 0x64: // ESC d n -- feed lines
                            if (RequireBytes(span, i, 3) is { } t13) return Result.Error<PrintProgram>(t13);
                            instructions.Add(new FeedLinesInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x69: // ESC i -- half cut
                            instructions.Add(new HalfCutInstruction());
                            i += 2;
                            break;

                        case 0x70: // ESC p m t1 t2 -- generate pulse
                            if (RequireBytes(span, i, 5) is { } t14) return Result.Error<PrintProgram>(t14);
                            instructions.Add(new GeneratePulseInstruction((ConnectorPin)span[i + 2], span[i + 3], span[i + 4]));
                            i += 5;
                            break;

                        case 0x74: // ESC t n -- select code page
                            if (RequireBytes(span, i, 3) is { } t15) return Result.Error<PrintProgram>(t15);
                            instructions.Add(new SelectCodePageInstruction((CharacterCodePage)span[i + 2]));
                            i += 3;
                            break;

                        case 0x7B: // ESC { n -- upside-down
                            if (RequireBytes(span, i, 3) is { } t16) return Result.Error<PrintProgram>(t16);
                            instructions.Add(new UpsideDownInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        default:
                            return Result.Error<PrintProgram>(new UnrecognizedBytesProblem(
                                i, $"ESC 0x{span[i + 1]:X2}"));
                    }
                    break;

                case 0x1D: // GS
                    if (RequireBytes(span, i, 2) is { } truncGs)
                        return Result.Error<PrintProgram>(truncGs);
                    switch (span[i + 1])
                    {
                        case 0x21: // GS ! n -- font size
                            if (RequireBytes(span, i, 3) is { } t17) return Result.Error<PrintProgram>(t17);
                            instructions.Add(new FontSizeInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x42: // GS B n -- reverse
                            if (RequireBytes(span, i, 3) is { } t18) return Result.Error<PrintProgram>(t18);
                            instructions.Add(new ReverseInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        case 0x4C: // GS L nL nH -- left margin
                            if (RequireBytes(span, i, 4) is { } t19) return Result.Error<PrintProgram>(t19);
                            instructions.Add(new LeftMarginInstruction((ushort)(span[i + 2] | (span[i + 3] << 8))));
                            i += 4;
                            break;

                        case 0x56: // GS V -- cut variants
                            if (RequireBytes(span, i, 3) is { } t20) return Result.Error<PrintProgram>(t20);
                            switch (span[i + 2])
                            {
                                case 0x01: // GS V 1 -- CutInstruction
                                    instructions.Add(new CutInstruction());
                                    i += 3;
                                    break;
                                case 0x42: // GS V 66 n -- CutAfterInstruction
                                    if (RequireBytes(span, i, 4) is { } t21) return Result.Error<PrintProgram>(t21);
                                    instructions.Add(new CutAfterInstruction(span[i + 3]));
                                    i += 4;
                                    break;
                                default:
                                    return Result.Error<PrintProgram>(new UnrecognizedBytesProblem(
                                        i + 2, $"GS V 0x{span[i + 2]:X2}"));
                            }
                            break;

                        case 0x57: // GS W nL nH -- print area width
                            if (RequireBytes(span, i, 4) is { } t22) return Result.Error<PrintProgram>(t22);
                            instructions.Add(new PrintAreaWidthInstruction((ushort)(span[i + 2] | (span[i + 3] << 8))));
                            i += 4;
                            break;

                        default:
                            return Result.Error<PrintProgram>(new UnrecognizedBytesProblem(
                                i, $"GS 0x{span[i + 1]:X2}"));
                    }
                    break;

                case >= 0x20 and <= 0x7E: // printable ASCII
                    var start = i;
                    while (i < span.Length && span[i] >= 0x20 && span[i] <= 0x7E)
                        i++;
                    instructions.Add(new TextInstruction(Encoding.ASCII.GetString(span[start..i])));
                    break;

                default:
                    return Result.Error<PrintProgram>(new UnrecognizedBytesProblem(
                        i, $"0x{span[i]:X2}"));
            }
        }

        return Result.Ok(new PrintProgram(instructions.ToImmutable()));
    }

    private static TruncatedSequenceProblem? RequireBytes(ReadOnlySpan<byte> span, int offset, int count)
    {
        if (offset + count > span.Length)
            return new TruncatedSequenceProblem(offset, count, span.Length - offset);
        return null;
    }
}