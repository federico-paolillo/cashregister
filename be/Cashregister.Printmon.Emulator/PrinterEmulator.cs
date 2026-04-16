using System.Collections.Immutable;

using Cashregister.Factories;

namespace Cashregister.Printmon.Emulator;

public interface IPrinterEmulator
{
    Result<ImmutableArray<Printer>> Emulate(ReadOnlyMemory<byte> escPosData);
}

public sealed class PrinterEmulator(
    IInstructionDecoder decoder,
    IInstructionExecutor executor) : IPrinterEmulator
{
    public Result<ImmutableArray<Printer>> Emulate(ReadOnlyMemory<byte> escPosData)
    {
        var decodeResult = decoder.Decode(escPosData);
        if (decodeResult.NotOk)
            return Result.Error<ImmutableArray<Printer>>(decodeResult.Error);

        var printer = Printer.Default;
        var history = ImmutableArray.CreateBuilder<Printer>();

        foreach (var instruction in decodeResult.Value.Instructions)
        {
            var executeResult = executor.Execute(printer, instruction);
            if (executeResult.NotOk)
                return Result.Error<ImmutableArray<Printer>>(executeResult.Error);

            printer = executeResult.Value;
            history.Add(printer);
        }

        return Result.Ok(history.ToImmutable());
    }
}