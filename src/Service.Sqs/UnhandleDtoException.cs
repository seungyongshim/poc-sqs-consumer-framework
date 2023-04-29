using System.Runtime.Serialization;

namespace Service.Sqs;

[Serializable]
internal class UnhandleDtoException : Exception
{
    public UnhandleDtoException(object a) : base($"Unhandle dto: {a.GetType().Name}")
    {
    }

    public UnhandleDtoException(string? message) : base(message)
    {
    }

    public UnhandleDtoException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected UnhandleDtoException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
