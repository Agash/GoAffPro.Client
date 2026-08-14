using System.Diagnostics.CodeAnalysis;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;

namespace GoAffPro.Client.Kiota;

/// <summary>
/// Normalizes GoAffPro date-filter query parameters to the preferred UTC wire form.
/// </summary>
internal sealed class StrictUtcDateQueryRequestAdapter(IRequestAdapter inner) : IRequestAdapter, IDisposable, IAsyncDisposable
{
    private static readonly HashSet<string> _dateQueryParameterNames =
    [
        "created_at_max",
        "created_at_min",
        "end_time",
        "start_time",
    ];

    private readonly IRequestAdapter _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public string? BaseUrl
    {
        get => _inner.BaseUrl;
        set => _inner.BaseUrl = value;
    }

    public ISerializationWriterFactory SerializationWriterFactory => _inner.SerializationWriterFactory;

    public void EnableBackingStore(IBackingStoreFactory backingStoreFactory)
    {
        _inner.EnableBackingStore(backingStoreFactory);
    }

    public Task<ModelType?> SendAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        NormalizeDateQueryParameters(requestInfo);
        return _inner.SendAsync(requestInfo, factory, errorMapping, cancellationToken);
    }

    public Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(
        RequestInformation requestInfo,
        ParsableFactory<ModelType> factory,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default) where ModelType : IParsable
    {
        NormalizeDateQueryParameters(requestInfo);
        return _inner.SendCollectionAsync(requestInfo, factory, errorMapping, cancellationToken);
    }

    // The primitive overloads keep the interface's PublicFields annotation so the trimmer keeps enum
    // members reachable: Kiota parses enum primitives by field name. A pass-through wrapper has to
    // restate it, because the analyzer compares annotations per declaration, not through the forward.
    public Task<ModelType?> SendPrimitiveAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] ModelType>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        NormalizeDateQueryParameters(requestInfo);
        return _inner.SendPrimitiveAsync<ModelType>(requestInfo, errorMapping, cancellationToken);
    }

    public Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] ModelType>(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        NormalizeDateQueryParameters(requestInfo);
        return _inner.SendPrimitiveCollectionAsync<ModelType>(requestInfo, errorMapping, cancellationToken);
    }

    public Task SendNoContentAsync(
        RequestInformation requestInfo,
        Dictionary<string, ParsableFactory<IParsable>>? errorMapping = null,
        CancellationToken cancellationToken = default)
    {
        NormalizeDateQueryParameters(requestInfo);
        return _inner.SendNoContentAsync(requestInfo, errorMapping, cancellationToken);
    }

    public Task<T?> ConvertToNativeRequestAsync<T>(RequestInformation requestInfo, CancellationToken cancellationToken = default)
    {
        NormalizeDateQueryParameters(requestInfo);
        return _inner.ConvertToNativeRequestAsync<T>(requestInfo, cancellationToken);
    }

    public void Dispose()
    {
        if (_inner is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public ValueTask DisposeAsync()
    {
        return _inner is IAsyncDisposable asyncDisposable
            ? asyncDisposable.DisposeAsync()
            : ValueTask.CompletedTask;
    }

    private static void NormalizeDateQueryParameters(RequestInformation requestInfo)
    {
        ArgumentNullException.ThrowIfNull(requestInfo);

        foreach (string parameterName in _dateQueryParameterNames)
        {
            if (!requestInfo.QueryParameters.TryGetValue(parameterName, out object? rawValue))
            {
                continue;
            }

            if (rawValue is not DateTimeOffset dateTimeOffset)
            {
                continue;
            }

            string normalized = GoAffProUtils.FormatTimestampQuery(dateTimeOffset);
            if (rawValue is string existing && string.Equals(existing, normalized, StringComparison.Ordinal))
            {
                continue;
            }

            requestInfo.QueryParameters[parameterName] = normalized;
        }
    }
}
