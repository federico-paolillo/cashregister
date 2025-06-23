using EscPosEmulator;

if (args.Length == 0)
{
#pragma warning disable CA1303 // Do not pass literals as localized parameters
    Console.WriteLine("Usage: EscPosEmulator.Cli <input-file>");
#pragma warning restore CA1303
    return;
}

string input = args[0];
if (!File.Exists(input))
{
#pragma warning disable CA1303
    Console.WriteLine($"Input file '{input}' not found.");
#pragma warning restore CA1303
    return;
}

byte[] data = await File.ReadAllBytesAsync(input);
EscPosHtmlConverter converter = new();
string html = converter.Convert(data);
string outputFile = Path.Combine(Directory.GetCurrentDirectory(), $"{Path.GetFileNameWithoutExtension(input)}-{Guid.NewGuid()}.html");
await File.WriteAllTextAsync(outputFile, html);
#pragma warning disable CA1303
Console.WriteLine($"HTML written to {outputFile}");
#pragma warning restore CA1303


