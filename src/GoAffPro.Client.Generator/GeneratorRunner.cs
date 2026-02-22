using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace GoAffPro.Client.Generator;

internal static class GeneratorRunner
{
    public const string DefaultSwaggerInitUrl = "https://api.goaffpro.com/docs/admin/swagger-ui-init.js";

    private const string UserPathPrefix = "/user";
    private const string PublicPathPrefix = "/public";
    private const string RootSpecFileName = "goaffpro.openapi.json";
    private const string UserSpecFileName = "goaffpro-user.openapi.json";
    private const string PublicSpecFileName = "goaffpro-public.openapi.json";
    private const string UserClientFileName = "GoAffProUserClient.g.cs";
    private const string PublicClientFileName = "GoAffProPublicClient.g.cs";
    private const string HashFileName = "GoAffPro.Client.Generator.hash";
    private const string LockFileName = "GoAffPro.Client.Generator.lock";
    private const string GeneratorCacheVersion = "3";

    public static async Task RunAsync(GeneratorOptions options, CancellationToken cancellationToken)
    {
        string projectDirectory = Path.GetFullPath(options.ProjectDirectory);
        string repositoryRoot = Path.GetFullPath(Path.Combine(projectDirectory, "..", ".."));
        string openApiDirectory = Path.Combine(repositoryRoot, "openapi");
        string generatedDirectory = Path.Combine(projectDirectory, "Generated");
        string intermediateDirectory = Path.Combine(projectDirectory, "obj");

        _ = Directory.CreateDirectory(openApiDirectory);
        _ = Directory.CreateDirectory(generatedDirectory);
        _ = Directory.CreateDirectory(intermediateDirectory);

        string lockFilePath = Path.Combine(intermediateDirectory, LockFileName);
        using FileStream generatorLock = await AcquireGeneratorLockAsync(lockFilePath, TimeSpan.FromMinutes(2), cancellationToken)
            .ConfigureAwait(false);

        try
        {
            (string swaggerUiInitContents, string swaggerSource) =
                await LoadSwaggerUiInitContentsAsync(options, projectDirectory, cancellationToken).ConfigureAwait(false);

            string swaggerDocumentJson = SpecExtractor.ExtractSwaggerDocumentJson(swaggerUiInitContents);
            JsonNode parsedNode = JsonNode.Parse(swaggerDocumentJson)
                ?? throw new InvalidOperationException("Failed to parse swagger document JSON.");
            JsonObject rootSpec = parsedNode.AsObject();

            JsonSerializerOptions writeIndented = new() { WriteIndented = true };
            string normalizedRootJson = rootSpec.ToJsonString(writeIndented);
            string canonicalRootJson = rootSpec.ToJsonString();

            string rootSpecPath = Path.Combine(openApiDirectory, RootSpecFileName);
            await File.WriteAllTextAsync(rootSpecPath, normalizedRootJson, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

            string hashPath = Path.Combine(intermediateDirectory, HashFileName);
            string currentHash = ComputeSha256($"{GeneratorCacheVersion}\n{canonicalRootJson}");
            bool isSpecUnchanged = File.Exists(hashPath) &&
                                   string.Equals(
                                       await File.ReadAllTextAsync(hashPath, cancellationToken).ConfigureAwait(false),
                                       currentHash,
                                       StringComparison.Ordinal);

            string userClientOutputPath = Path.Combine(generatedDirectory, UserClientFileName);
            string publicClientOutputPath = Path.Combine(generatedDirectory, PublicClientFileName);
            if (isSpecUnchanged && File.Exists(userClientOutputPath) && File.Exists(publicClientOutputPath))
            {
                return;
            }

            JsonObject userSpec = NormalizeSpec(FilterPaths(rootSpec, UserPathPrefix));
            JsonObject publicSpec = NormalizeSpec(FilterPaths(rootSpec, PublicPathPrefix));

            string userSpecJson = userSpec.ToJsonString(writeIndented);
            string publicSpecJson = publicSpec.ToJsonString(writeIndented);

            string userSpecPath = Path.Combine(openApiDirectory, UserSpecFileName);
            string publicSpecPath = Path.Combine(openApiDirectory, PublicSpecFileName);
            await File.WriteAllTextAsync(userSpecPath, userSpecJson, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            await File.WriteAllTextAsync(publicSpecPath, publicSpecJson, Encoding.UTF8, cancellationToken).ConfigureAwait(false);

            string userClientCode = await GenerateClientAsync(
                    userSpecJson,
                    @namespace: "GoAffPro.Client.Generated.User",
                    className: "GoAffProUserClient",
                    cancellationToken)
                .ConfigureAwait(false);
            string publicClientCode = await GenerateClientAsync(
                    publicSpecJson,
                    @namespace: "GoAffPro.Client.Generated.Public",
                    className: "GoAffProPublicClient",
                    cancellationToken)
                .ConfigureAwait(false);

            await File.WriteAllTextAsync(
                    userClientOutputPath,
                    AddAutoGeneratedHeader(userClientCode, swaggerSource),
                    Encoding.UTF8,
                    cancellationToken)
                .ConfigureAwait(false);
            await File.WriteAllTextAsync(
                    publicClientOutputPath,
                    AddAutoGeneratedHeader(publicClientCode, swaggerSource),
                    Encoding.UTF8,
                    cancellationToken)
                .ConfigureAwait(false);

            await File.WriteAllTextAsync(hashPath, currentHash, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // Lock is released by disposing generatorLock.
        }
    }

    private static async Task<FileStream> AcquireGeneratorLockAsync(string lockFilePath, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return new FileStream(lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (stopwatch.Elapsed < timeout)
            {
                await Task.Delay(150, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async Task<(string Contents, string SourceReference)> LoadSwaggerUiInitContentsAsync(
        GeneratorOptions options,
        string projectDirectory,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.SwaggerInitPath))
        {
            string configuredPath = Path.GetFullPath(options.SwaggerInitPath);
            if (File.Exists(configuredPath))
            {
                return (await File.ReadAllTextAsync(configuredPath, cancellationToken).ConfigureAwait(false), configuredPath);
            }
        }

        string defaultLocalPath = Path.GetFullPath(Path.Combine(projectDirectory, "..", "..", "openapi", "swagger-ui-init.js"));
        if (File.Exists(defaultLocalPath))
        {
            return (await File.ReadAllTextAsync(defaultLocalPath, cancellationToken).ConfigureAwait(false), defaultLocalPath);
        }

        using var client = new HttpClient();
        string remoteContents = await client.GetStringAsync(options.SwaggerUrl, cancellationToken).ConfigureAwait(false);
        return (remoteContents, options.SwaggerUrl.ToString());
    }

    private static JsonObject FilterPaths(JsonObject rootSpec, string pathPrefix)
    {
        var clone = (JsonObject)rootSpec.DeepClone();
        JsonObject originalPaths = clone["paths"]?.AsObject()
            ?? throw new InvalidOperationException("OpenAPI document does not contain a 'paths' object.");

        JsonObject filteredPaths = [];
        foreach ((string path, JsonNode? pathDefinition) in originalPaths)
        {
            if (path.Equals(pathPrefix, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(pathPrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                filteredPaths[path] = pathDefinition?.DeepClone();
            }
        }

        clone["paths"] = filteredPaths;
        return clone;
    }

    private static JsonObject NormalizeSpec(JsonObject spec)
    {
        if (spec["paths"] is not JsonObject paths)
        {
            return spec;
        }

        foreach ((string path, JsonNode? pathNode) in paths)
        {
            if (pathNode is not JsonObject pathItem)
            {
                continue;
            }

            foreach (string methodName in new[] { "get", "put", "post", "delete", "patch", "options", "head", "trace" })
            {
                if (pathItem[methodName] is not JsonObject operation)
                {
                    continue;
                }

                EnsureOperationResponses(path, operation);
                NormalizeParameters(operation);
            }
        }

        return spec;
    }

    private static void EnsureOperationResponses(string path, JsonObject operation)
    {
        if (operation["responses"] is not JsonObject responses || responses.Count == 0)
        {
            operation["responses"] = CreateSuccessResponse(path);
            return;
        }

        foreach ((string statusCode, JsonNode? responseNode) in responses.ToList())
        {
            if (responseNode is null)
            {
                responses[statusCode] = CreateSuccessResponseNode(path);
            }
        }

        if (responses.TryGetPropertyValue("200", out JsonNode? successNode) && successNode is JsonObject successResponse)
        {
            EnsureSuccessResponseHasJsonSchema(path, successResponse);
        }
    }

    private static void NormalizeParameters(JsonObject operation)
    {
        if (operation["parameters"] is not JsonArray parameters)
        {
            return;
        }

        foreach (JsonNode? parameterNode in parameters)
        {
            if (parameterNode is not JsonObject parameter)
            {
                continue;
            }

            if (parameter["schema"] is not null || !parameter.TryGetPropertyValue("type", out JsonNode? typeNode))
            {
                continue;
            }

            JsonObject schema = [];
            if (typeNode is not null)
            {
                schema["type"] = typeNode.DeepClone();
                _ = parameter.Remove("type");
            }

            if (parameter.TryGetPropertyValue("enum", out JsonNode? enumNode) && enumNode is not null)
            {
                schema["enum"] = enumNode.DeepClone();
                _ = parameter.Remove("enum");
            }

            if (parameter.TryGetPropertyValue("items", out JsonNode? itemsNode) && itemsNode is not null)
            {
                schema["items"] = itemsNode.DeepClone();
                _ = parameter.Remove("items");
            }

            parameter["schema"] = schema;
        }
    }

    private static JsonObject CreateSuccessResponse(string path)
    {
        return new JsonObject
        {
            ["200"] = CreateSuccessResponseNode(path),
        };
    }

    private static JsonObject CreateSuccessResponseNode(string path)
    {
        return new JsonObject
        {
            ["description"] = "Success",
            ["content"] = CreateJsonContentNode(path),
        };
    }

    private static void EnsureSuccessResponseHasJsonSchema(string path, JsonObject response)
    {
        if (RequiresFeedSchemaOverride(path))
        {
            response["content"] = CreateJsonContentNode(path);
            return;
        }

        if (response["content"] is not JsonObject content)
        {
            response["content"] = CreateJsonContentNode(path);
            return;
        }

        if (content.TryGetPropertyValue("application/json", out JsonNode? mediaTypeNode) &&
            mediaTypeNode is JsonObject mediaTypeObject &&
            mediaTypeObject["schema"] is not null)
        {
            return;
        }

        content["application/json"] = new JsonObject
        {
            ["schema"] = CreateResponseSchema(path),
        };
    }

    private static bool RequiresFeedSchemaOverride(string path)
    {
        return path is "/user/feed/orders" or "/user/feed/traffic" or "/user/feed/rewards";
    }

    private static JsonObject CreateJsonContentNode(string path)
    {
        return new JsonObject
        {
            ["application/json"] = new JsonObject
            {
                ["schema"] = CreateResponseSchema(path),
            },
        };
    }

    private static JsonObject CreateResponseSchema(string path)
    {
        string? feedProperty = path switch
        {
            "/user/feed/orders" => "orders",
            "/user/feed/traffic" => "traffic",
            "/user/feed/rewards" => "rewards",
            _ => null,
        };

        return feedProperty is null
            ? new JsonObject
            {
                ["type"] = "object",
                ["additionalProperties"] = true,
            }
            : new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    [feedProperty] = new JsonObject
                    {
                        ["type"] = "array",
                        ["items"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["additionalProperties"] = true,
                        },
                    },
                    ["limit"] = new JsonObject
                    {
                        ["type"] = "integer",
                    },
                    ["offset"] = new JsonObject
                    {
                        ["type"] = "integer",
                    },
                    ["count"] = new JsonObject
                    {
                        ["type"] = "integer",
                    },
                },
                ["additionalProperties"] = true,
            };
    }

    private static async Task<string> GenerateClientAsync(
        string openApiJson,
        string @namespace,
        string className,
        CancellationToken cancellationToken)
    {
        OpenApiDocument document = await OpenApiDocument.FromJsonAsync(openApiJson, cancellationToken).ConfigureAwait(false);
        CSharpClientGeneratorSettings settings = new()
        {
            ClassName = className,
            GenerateClientClasses = true,
            GenerateClientInterfaces = false,
            InjectHttpClient = true,
            DisposeHttpClient = false,
            GenerateBaseUrlProperty = true,
            UseBaseUrl = true,
            OperationNameGenerator = new SingleClientFromPathSegmentsOperationNameGenerator(),
            GenerateExceptionClasses = true,
            ExceptionClass = className + "ApiException",
            GenerateResponseClasses = true,
            ResponseClass = className + "SwaggerResponse",
            CSharpGeneratorSettings =
            {
                Namespace = @namespace,
                JsonLibrary = CSharpJsonLibrary.SystemTextJson,
                GenerateNullableReferenceTypes = true,
                ClassStyle = CSharpClassStyle.Poco,
            },
        };

        CSharpClientGenerator generator = new(document, settings);
        return generator.GenerateFile();
    }

    private static string AddAutoGeneratedHeader(string generatedCode, string source)
    {
        StringBuilder builder = new();
        _ = builder.AppendLine("// <auto-generated>");
        _ = builder.AppendLine("// This file was generated by GoAffPro.Client.Generator.");
        _ = builder.Append("// Source: ").AppendLine(source);
        _ = builder.Append("// Generated: ").AppendLine(DateTimeOffset.UtcNow.ToString("O"));
        _ = builder.AppendLine("// Do not edit manually.");
        _ = builder.AppendLine("// </auto-generated>");
        _ = builder.AppendLine();
        _ = builder.Append(generatedCode);
        return builder.ToString();
    }

    private static string ComputeSha256(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
