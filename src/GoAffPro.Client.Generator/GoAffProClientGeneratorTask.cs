using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace GoAffPro.Client.Generator;

public sealed class GoAffProClientGeneratorTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public string ProjectDirectory { get; set; } = string.Empty;

    public string? SwaggerInitPath { get; set; }

    public Uri SwaggerUrl { get; set; } = new(GeneratorRunner.DefaultSwaggerInitUrl, UriKind.Absolute);

    public override bool Execute()
    {
        var options = new GeneratorOptions(ProjectDirectory, SwaggerInitPath, SwaggerUrl);
        GeneratorRunner.RunAsync(options, CancellationToken.None).GetAwaiter().GetResult();
        return true;
    }
}
