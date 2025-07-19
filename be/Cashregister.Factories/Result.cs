using System.Diagnostics.CodeAnalysis;

namespace Cashregister.Factories;

public abstract record Problem;

public sealed class Result<TValue>
{
    public TValue? Value { get; }

    public Problem? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Ok => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool NotOk => Error is not null;

    internal Result(TValue value)
    {
        Value = value;
        Error = null;
    }

    internal Result(Problem error)
    {
        Value = default;
        Error = error;
    }
}

public static class Result
{
    public static Result<TOutput> Ok<TOutput>(TOutput result) where TOutput : notnull
    {
        return new Result<TOutput>(result);
    }

    public static Result<TOutput> Error<TOutput>(Problem problem) where TOutput : notnull
    {
        return new Result<TOutput>(problem);
    }

    public static Result<Unit> Void()
    {
        return new Result<Unit>(value: default);
    }

    public static Result<Unit> Error(Problem problem)
    {
        return new Result<Unit>(problem);
    }
}