namespace PocBaseResponseHandler.Exceptions;

using System.Runtime.Serialization;

[Serializable]
internal sealed class CustomAppException : ApplicationException
{
    public override string Code => "custom_exception_code";

    public CustomAppException(string message)
        : base(message)
    {
    }

    private CustomAppException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
