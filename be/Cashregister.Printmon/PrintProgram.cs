using System.Collections.Immutable;

using Cashregister.Printmon.Instructions;

namespace Cashregister.Printmon;

/// <summary>
/// Represents a list of <see cref="Instruction"/> that compose a complete printer program.<br/>
/// May contain extra metadata needed for bookkeeping.
/// </summary>
public sealed record PrintProgram(ImmutableArray<Instruction> Instructions);