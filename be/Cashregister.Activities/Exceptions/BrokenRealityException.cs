namespace Cashregister.Activities.Exceptions;

public sealed class BrokenRealityException : Exception
{
    public BrokenRealityException()
    {
    }

    public BrokenRealityException(string message)
        : base(message)
    {
    }

    public BrokenRealityException(string message, Exception inner)
        : base(message, inner)
    {
    }
}