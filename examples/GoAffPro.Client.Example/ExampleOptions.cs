namespace GoAffPro.Client.Example;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Instantiated via options binding.")]
internal sealed class ExampleOptions
{
    public string? Email { get; set; }

    public string? Password { get; set; }

    public int PollingIntervalSeconds { get; set; } = 30;

    public int PageSize { get; set; } = 50;
}
