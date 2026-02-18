using System.Net;

namespace GoAffPro.Client.Exceptions;

public sealed class GoAffProApiException : Exception
{
    public GoAffProApiException()
    {
    }

    public GoAffProApiException(string message)
        : base(message)
    {
    }

    public GoAffProApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public GoAffProApiException(string message, HttpStatusCode statusCode, string? responseBody = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string? ResponseBody { get; }
}
