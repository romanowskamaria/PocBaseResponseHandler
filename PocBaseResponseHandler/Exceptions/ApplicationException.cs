namespace PocBaseResponseHandler.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public abstract class ApplicationException : Exception
{
    public abstract string Code { get; }

    protected ApplicationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    protected ApplicationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
