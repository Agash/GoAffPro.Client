using System.Net;

namespace GoAffPro.Client.Exceptions;

/// <summary>
/// Represents an error returned by the GoAffPro API.
/// </summary>
public sealed class GoAffProApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProApiException"/> class.
    /// </summary>
    public GoAffProApiException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProApiException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public GoAffProApiException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProApiException"/> class
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Inner exception that caused this failure.</param>
    public GoAffProApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GoAffProApiException"/> class
    /// with status and response information.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="statusCode">HTTP status code returned by the API.</param>
    /// <param name="responseBody">Raw response body returned by the API, when available.</param>
    /// <param name="innerException">Underlying exception, when available.</param>
    public GoAffProApiException(string message, HttpStatusCode statusCode, string? responseBody = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the raw response body returned by the API, when available.
    /// </summary>
    public string? ResponseBody { get; }
}
