using System.Collections.Immutable;
using System.Text;

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
    ImmutableArray<Instruction> Decode(ReadOnlyMemory<byte> data);
}

public sealed class InstructionDecoder : IInstructionDecoder
{
    public ImmutableArray<Instruction> Decode(ReadOnlyMemory<byte> data)
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
                    RequireBytes(span, i, 5);
                    if (span[i + 1] != 0x14 || span[i + 2] != 0x01)
                        throw new InvalidOperationException(
                            $"Unknown ESC/POS sequence 0x{span[i]:X2} 0x{span[i + 1]:X2} 0x{span[i + 2]:X2} at offset {i}.");
                    instructions.Add(new RealTimePulseInstruction((ConnectorPin)span[i + 3], span[i + 4]));
                    i += 5;
                    break;

                case 0x1B: // ESC
                    RequireBytes(span, i, 2);
                    switch (span[i + 1])
                    {
                        case 0x40: // ESC @ -- Initialize
                            instructions.Add(new InitializeInstruction());
                            i += 2;
                            break;

                        case 0x21: // ESC ! n -- print mode / reset print mode
                            RequireBytes(span, i, 3);
                            var printModeN = span[i + 2];
                            instructions.Add(printModeN == 0
                                ? new ResetPrintModeInstruction()
                                : new SelectPrintModeInstruction((printModeN & 0x01) != 0, (FormatMode)(printModeN & ~0x01)));
                            i += 3;
                            break;

                        case 0x20: // ESC SP n -- right spacing
                            RequireBytes(span, i, 3);
                            instructions.Add(new RightSpacingInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x24: // ESC $ nL nH -- absolute position
                            RequireBytes(span, i, 4);
                            instructions.Add(new AbsolutePositionInstruction((ushort)(span[i + 2] | (span[i + 3] << 8))));
                            i += 4;
                            break;

                        case 0x2D: // ESC - n -- underline
                            RequireBytes(span, i, 3);
                            var underlineN = span[i + 2];
                            instructions.Add(new UnderlineInstruction(underlineN != 0, (Thickness)underlineN));
                            i += 3;
                            break;

                        case 0x32: // ESC 2 -- reset line spacing
                            instructions.Add(new ResetLineSpacingInstruction());
                            i += 2;
                            break;

                        case 0x33: // ESC 3 n -- set line spacing
                            RequireBytes(span, i, 3);
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
                                throw new InvalidOperationException(
                                    $"Truncated ESC D sequence: missing NUL terminator at offset {i}.");
                            i++; // consume NUL
                            instructions.Add(new SetHorizontalTabsInstruction(tabPositions.ToImmutable()));
                            break;

                        case 0x45: // ESC E n -- emphasize
                            RequireBytes(span, i, 3);
                            instructions.Add(new EmphasizeInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        case 0x47: // ESC G n -- double-strike
                            RequireBytes(span, i, 3);
                            instructions.Add(new DoubleStrikeInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        case 0x4A: // ESC J n -- feed paper
                            RequireBytes(span, i, 3);
                            instructions.Add(new FeedPaperInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x4D: // ESC M n -- select font
                            RequireBytes(span, i, 3);
                            instructions.Add(new SelectFontInstruction((CharacterFont)span[i + 2]));
                            i += 3;
                            break;

                        case 0x56: // ESC V n -- rotation
                            RequireBytes(span, i, 3);
                            instructions.Add(new RotationInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        case 0x5C: // ESC \ nL nH -- relative position
                            RequireBytes(span, i, 4);
                            instructions.Add(new RelativePositionInstruction((ushort)(span[i + 2] | (span[i + 3] << 8))));
                            i += 4;
                            break;

                        case 0x61: // ESC a n -- justification
                            RequireBytes(span, i, 3);
                            instructions.Add(new JustifyInstruction((Alignment)span[i + 2]));
                            i += 3;
                            break;

                        case 0x64: // ESC d n -- feed lines
                            RequireBytes(span, i, 3);
                            instructions.Add(new FeedLinesInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x69: // ESC i -- half cut
                            instructions.Add(new HalfCutInstruction());
                            i += 2;
                            break;

                        case 0x70: // ESC p m t1 t2 -- generate pulse
                            RequireBytes(span, i, 5);
                            instructions.Add(new GeneratePulseInstruction((ConnectorPin)span[i + 2], span[i + 3], span[i + 4]));
                            i += 5;
                            break;

                        case 0x74: // ESC t n -- select code page
                            RequireBytes(span, i, 3);
                            instructions.Add(new SelectCodePageInstruction((CharacterCodePage)span[i + 2]));
                            i += 3;
                            break;

                        case 0x7B: // ESC { n -- upside-down
                            RequireBytes(span, i, 3);
                            instructions.Add(new UpsideDownInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        default:
                            throw new InvalidOperationException(
                                $"Unknown ESC/POS sequence ESC 0x{span[i + 1]:X2} at offset {i}.");
                    }
                    break;

                case 0x1D: // GS
                    RequireBytes(span, i, 2);
                    switch (span[i + 1])
                    {
                        case 0x21: // GS ! n -- font size
                            RequireBytes(span, i, 3);
                            instructions.Add(new FontSizeInstruction(span[i + 2]));
                            i += 3;
                            break;

                        case 0x42: // GS B n -- reverse
                            RequireBytes(span, i, 3);
                            instructions.Add(new ReverseInstruction(span[i + 2] != 0));
                            i += 3;
                            break;

                        case 0x4C: // GS L nL nH -- left margin
                            RequireBytes(span, i, 4);
                            instructions.Add(new LeftMarginInstruction((ushort)(span[i + 2] | (span[i + 3] << 8))));
                            i += 4;
                            break;

                        case 0x56: // GS V -- cut variants
                            RequireBytes(span, i, 3);
                            switch (span[i + 2])
                            {
                                case 0x01: // GS V 1 -- CutInstruction
                                    instructions.Add(new CutInstruction());
                                    i += 3;
                                    break;
                                case 0x42: // GS V 66 n -- CutAfterInstruction
                                    RequireBytes(span, i, 4);
                                    instructions.Add(new CutAfterInstruction(span[i + 3]));
                                    i += 4;
                                    break;
                                default:
                                    throw new InvalidOperationException(
                                        $"Unknown GS V subcommand 0x{span[i + 2]:X2} at offset {i + 2}.");
                            }
                            break;

                        case 0x57: // GS W nL nH -- print area width
                            RequireBytes(span, i, 4);
                            instructions.Add(new PrintAreaWidthInstruction((ushort)(span[i + 2] | (span[i + 3] << 8))));
                            i += 4;
                            break;

                        default:
                            throw new InvalidOperationException(
                                $"Unknown ESC/POS sequence GS 0x{span[i + 1]:X2} at offset {i}.");
                    }
                    break;

                case >= 0x20 and <= 0x7E: // printable ASCII
                    var start = i;
                    while (i < span.Length && span[i] >= 0x20 && span[i] <= 0x7E)
                        i++;
                    instructions.Add(new TextInstruction(Encoding.ASCII.GetString(span[start..i])));
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown ESC/POS byte 0x{span[i]:X2} at offset {i}.");
            }
        }

        return instructions.ToImmutable();
    }

    private static void RequireBytes(ReadOnlySpan<byte> span, int offset, int count)
    {
        if (offset + count > span.Length)
            throw new InvalidOperationException(
                $"Truncated ESC/POS sequence at offset {offset}: expected {count} bytes, only {span.Length - offset} available.");
    }
}
