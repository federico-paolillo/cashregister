using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Receipts.Problems;

/// <summary>
///     Problem returned when receipt print data cannot be found for an order.
/// </summary>
public sealed record NoSuchOrderPrintDataProblem(Identifier OrderId) : Problem;