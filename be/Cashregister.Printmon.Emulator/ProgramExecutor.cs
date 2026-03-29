using System.Collections.Immutable;

using Cashregister.Printmon.Instructions;

namespace Cashregister.Printmon.Emulator;

public interface IProgramExecutor
{
    DocumentIr Execute(ReadOnlyMemory<byte> escPosData);

    ImmutableArray<PrinterDocument> ExecuteWithHistory(
        IEnumerable<Instruction> instructions, PrinterState? initialState = null);
}

public sealed class ProgramExecutor(
    IInstructionDecoder decoder,
    IInstructionExecutor executor) : IProgramExecutor
{
    public DocumentIr Execute(ReadOnlyMemory<byte> escPosData)
    {
        var instructions = decoder.Decode(escPosData);
        var elements = ImmutableArray.CreateBuilder<IDocumentElement>();
        var state = PrinterState.Default;

        foreach (var instruction in instructions)
        {
            var doc = executor.Execute(state, instruction);
            state = doc.State;
            elements.AddRange(doc.Elements);
        }

        return new DocumentIr(elements.ToImmutable());
    }

    public ImmutableArray<PrinterDocument> ExecuteWithHistory(
        IEnumerable<Instruction> instructions, PrinterState? initialState = null)
    {
        ArgumentNullException.ThrowIfNull(instructions);

        var state = initialState ?? PrinterState.Default;
        var history = ImmutableArray.CreateBuilder<PrinterDocument>();

        foreach (var instruction in instructions)
        {
            var doc = executor.Execute(state, instruction);
            history.Add(doc);
            state = doc.State;
        }

        return history.ToImmutable();
    }
}
