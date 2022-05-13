namespace PocBaseResponseHandler.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public sealed class UnhandledErrorException : ApplicationException
{
    public override string Code => "unhandled_error";

    public UnhandledErrorException()
        : base("Unhandled error has occurred")
    {
    }

    private UnhandledErrorException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
