using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace GoAffPro.Client.Tests;

public sealed class GeneratedClientContractTests
{
    [Fact]
    public void GeneratedClientSignatures_WhenComparedToSnapshot_MatchCommittedSnapshot()
    {
        string repositoryRoot = FindRepositoryRoot();
        string generatedDirectory = Path.Combine(repositoryRoot, "src", "GoAffPro.Client.Generated", "Generated");
        string snapshotPath = Path.Combine(repositoryRoot, "tests", "GoAffPro.Client.Tests", "Snapshots", "GeneratedClientSignatures.snapshot");

        string actualSnapshot = BuildSnapshot(generatedDirectory);
        string expectedSnapshot = File.ReadAllText(snapshotPath, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal);

        _ = NormalizeSnapshot(actualSnapshot).Should().Be(NormalizeSnapshot(expectedSnapshot));
    }

    private static string BuildSnapshot(string generatedDirectory)
    {
        string[] files = Directory.GetFiles(generatedDirectory, "*.cs", SearchOption.AllDirectories);
        string userPathSegment = $"{Path.DirectorySeparatorChar}Generated{Path.DirectorySeparatorChar}User{Path.DirectorySeparatorChar}";
        string publicPathSegment = $"{Path.DirectorySeparatorChar}Generated{Path.DirectorySeparatorChar}Public{Path.DirectorySeparatorChar}";

        string[] userFiles = files
            .Where(path => path.Contains(userPathSegment, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        string[] publicFiles = files
            .Where(path => path.Contains(publicPathSegment, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var builder = new StringBuilder();
        AppendSignatureBlock(builder, "GoAffProUserClient", userFiles);
        _ = builder.AppendLine();
        AppendSignatureBlock(builder, "GoAffProPublicClient", publicFiles);
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static void AppendSignatureBlock(StringBuilder builder, string name, IReadOnlyCollection<string> filePaths)
    {
        _ = builder.Append('[').Append(name).AppendLine("]");
        foreach (string signature in ExtractAsyncMethodSignatures(filePaths))
        {
            _ = builder.AppendLine(signature);
        }
    }

    private static string[] ExtractAsyncMethodSignatures(IReadOnlyCollection<string> filePaths)
    {
        var signatures = new List<string>();
        foreach (string filePath in filePaths.OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            string content = File.ReadAllText(filePath, Encoding.UTF8);
            MatchCollection matches = Regex.Matches(content, @"public(?:\s+virtual)?[^\r\n]*Async\([^\r\n]*\)");
            signatures.AddRange(matches.Select(static match => Regex.Replace(match.Value, @"\s+", " ").Trim()));
        }

        return signatures
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static signature => signature, StringComparer.Ordinal)
            .ToArray();
    }

    private static string FindRepositoryRoot()
    {
        string? current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "GoAffPro.Client.slnx")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root from test output directory.");
    }

    private static string NormalizeSnapshot(string snapshot)
    {
        string[] lines = snapshot.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var normalized = new StringBuilder();
        var currentSignatures = new List<string>();

        foreach (string rawLine in lines)
        {
            string line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.Length >= 2 && line[0] == '[' && line[^1] == ']')
            {
                AppendSortedBlock(normalized, currentSignatures);
                currentSignatures.Clear();

                _ = normalized.AppendLine(line);
                continue;
            }

            currentSignatures.Add(line);
        }

        AppendSortedBlock(normalized, currentSignatures);
        return normalized.ToString();
    }

    private static void AppendSortedBlock(StringBuilder builder, List<string> signatures)
    {
        foreach (string signature in signatures.OrderBy(static value => value, StringComparer.Ordinal))
        {
            _ = builder.AppendLine(signature);
        }
    }
}
