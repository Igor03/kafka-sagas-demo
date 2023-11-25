namespace Contracts.Exceptions;

[Serializable]
public class NotATransientException : Exception
{
    public NotATransientException() { }

    public NotATransientException(string message)
        : base(message) { }

    public NotATransientException(string message, Exception inner)
        : base(message, inner) { }
}