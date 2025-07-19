using Cashregister.Domain;
using Cashregister.Factories;

namespace Cashregister.Application.Common.Problems;

public sealed record NoSuchThingProblem(Identifier MissingThing) : Problem;