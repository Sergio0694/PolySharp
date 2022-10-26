namespace PolySharp.SourceGenerators.Constants;

/// <summary>
/// Exposes the available PolySharp MSBuild properties.
/// </summary>
internal static class PolySharpMSBuildProperties
{
    /// <summary>
    /// The MSBuild property for <see cref="Models.GenerationOptions.UsePublicAccessibilityForGeneratedTypes"/>.
    /// </summary>
    public const string UsePublicAccessibilityForGeneratedTypes = "PolySharpUsePublicAccessibilityForGeneratedTypes";

    /// <summary>
    /// The MSBuild property for <see cref="Models.GenerationOptions.IncludeRuntimeSupportedAttributes"/>.
    /// </summary>
    public const string IncludeRuntimeSupportedAttributes = "PolySharpIncludeRuntimeSupportedAttributes";

    /// <summary>
    /// The MSBuild property for <see cref="Models.GenerationOptions.ExcludeGeneratedTypes"/>.
    /// </summary>
    public const string ExcludeGeneratedTypes = "PolySharpExcludeGeneratedTypes";

    /// <summary>
    /// The MSBuild property for <see cref="Models.GenerationOptions.IncludeGeneratedTypes"/>.
    /// </summary>
    public const string IncludeGeneratedTypes = "PolySharpIncludeGeneratedTypes";
}
