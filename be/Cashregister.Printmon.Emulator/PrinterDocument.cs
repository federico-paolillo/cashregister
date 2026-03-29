using System.Collections.Immutable;

namespace Cashregister.Printmon.Emulator;

public sealed record PrinterDocument(
    PrinterState State,
    ImmutableArray<IDocumentElement> Elements);
