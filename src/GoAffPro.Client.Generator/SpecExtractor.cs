namespace GoAffPro.Client.Generator;

internal static class SpecExtractor
{
    public static string ExtractSwaggerDocumentJson(string swaggerUiInitContents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(swaggerUiInitContents);

        const string marker = "\"swaggerDoc\"";
        int markerIndex = swaggerUiInitContents.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            throw new InvalidOperationException("Could not find 'swaggerDoc' in swagger-ui-init.js.");
        }

        int colonIndex = swaggerUiInitContents.IndexOf(':', markerIndex + marker.Length);
        if (colonIndex < 0)
        {
            throw new InvalidOperationException("Could not find the ':' token after 'swaggerDoc'.");
        }

        int objectStartIndex = swaggerUiInitContents.IndexOf('{', colonIndex);
        if (objectStartIndex < 0)
        {
            throw new InvalidOperationException("Could not find the start of the swagger document JSON object.");
        }

        int objectEndIndex = FindObjectEndIndex(swaggerUiInitContents, objectStartIndex);
        return swaggerUiInitContents[objectStartIndex..(objectEndIndex + 1)];
    }

    private static int FindObjectEndIndex(string contents, int objectStartIndex)
    {
        bool insideString = false;
        bool isEscaped = false;
        int depth = 0;

        for (int index = objectStartIndex; index < contents.Length; index++)
        {
            char current = contents[index];

            if (insideString)
            {
                if (isEscaped)
                {
                    isEscaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    isEscaped = true;
                    continue;
                }

                if (current == '"')
                {
                    insideString = false;
                }

                continue;
            }

            if (current == '"')
            {
                insideString = true;
                continue;
            }

            if (current == '{')
            {
                depth++;
                continue;
            }

            if (current != '}')
            {
                continue;
            }

            depth--;
            if (depth == 0)
            {
                return index;
            }
        }

        throw new InvalidOperationException("Could not parse swagger document JSON object boundaries.");
    }
}
