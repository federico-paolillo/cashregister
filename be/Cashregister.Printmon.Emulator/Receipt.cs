using System.Collections.Immutable;

using Cashregister.Printmon.Instructions.CodePage;
using Cashregister.Printmon.Instructions.Formatting;
using Cashregister.Printmon.Instructions.Layout;

namespace Cashregister.Printmon.Emulator;

public sealed record Receipt(ImmutableArray<IDocumentElement> Elements);

public abstract record IDocumentElement;

public sealed record TextStyle(
    bool Bold,
    Thickness Underline,
    bool DoubleStrike,
    CharacterFont Font,
    bool Rotation,
    bool UpsideDown,
    bool Reverse,
    byte WidthMultiplier,
    byte HeightMultiplier,
    Alignment Justification);

public sealed record TextSpan(string Text, TextStyle Style) : IDocumentElement;

public sealed record LineBreak : IDocumentElement;

public sealed record FeedLines(byte Count) : IDocumentElement;

public sealed record HorizontalRule : IDocumentElement;

// Placeholders for future implementation
public sealed record Barcode : IDocumentElement;

public sealed record QrCode : IDocumentElement;

public sealed record Image : IDocumentElement;