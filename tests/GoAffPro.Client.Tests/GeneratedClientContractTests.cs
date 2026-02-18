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
        string userClientPath = Path.Combine(repositoryRoot, "src", "GoAffPro.Client", "Generated", "GoAffProUserClient.g.cs");
        string publicClientPath = Path.Combine(repositoryRoot, "src", "GoAffPro.Client", "Generated", "GoAffProPublicClient.g.cs");
        string snapshotPath = Path.Combine(repositoryRoot, "tests", "GoAffPro.Client.Tests", "Snapshots", "GeneratedClientSignatures.snapshot");

        string actualSnapshot = BuildSnapshot(userClientPath, publicClientPath);
        string expectedSnapshot = File.ReadAllText(snapshotPath, Encoding.UTF8).Replace("\r\n", "\n", StringComparison.Ordinal);

        actualSnapshot.Should().Be(expectedSnapshot);
    }

    private static string BuildSnapshot(string userClientPath, string publicClientPath)
    {
        var builder = new StringBuilder();
        AppendSignatureBlock(builder, "GoAffProUserClient", userClientPath);
        builder.AppendLine();
        AppendSignatureBlock(builder, "GoAffProPublicClient", publicClientPath);
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static void AppendSignatureBlock(StringBuilder builder, string name, string filePath)
    {
        builder.Append('[').Append(name).AppendLine("]");
        foreach (string signature in ExtractAsyncMethodSignatures(filePath))
        {
            builder.AppendLine(signature);
        }
    }

    private static string[] ExtractAsyncMethodSignatures(string filePath)
    {
        string content = File.ReadAllText(filePath, Encoding.UTF8);
        MatchCollection matches = Regex.Matches(content, @"public virtual[^\r\n]*Async\([^\r\n]*\)");
        return matches
            .Select(static match => Regex.Replace(match.Value, @"\s+", " ").Trim())
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
}
