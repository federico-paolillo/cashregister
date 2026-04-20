using Cashregister.Factories;

namespace Cashregister.Printmon.Devices.Problems;

public sealed record DeviceIoProblem(IOException Source) : Problem;