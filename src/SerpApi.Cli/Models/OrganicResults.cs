using System.Text.Json.Serialization;

namespace SerpApi.Cli.Models;

public record OrganicResult(
    [property: JsonPropertyName("position")] int? Position,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("link")] string? Link,
    [property: JsonPropertyName("redirect_link")] string? RedirectLink,
    [property: JsonPropertyName("displayed_link")] string? DisplayedLink,
    [property: JsonPropertyName("favicon")] string? Favicon,
    [property: JsonPropertyName("snippet")] string? Snippet,
    [property: JsonPropertyName("snippet_highlighted_words")] IReadOnlyList<string>? SnippetHighlightedWords,
    [property: JsonPropertyName("about_this_result")] AboutThisResult? AboutThisResult,
    [property: JsonPropertyName("about_page_link")] string? AboutPageLink,
    [property: JsonPropertyName("about_page_serpapi_link")] string? AboutPageSerpapiLink,
    [property: JsonPropertyName("source")] string? Source
);

public record AboutThisResult(
    [property: JsonPropertyName("source")] SourceInfo? Source,
    [property: JsonPropertyName("languages")] IReadOnlyList<string>? Languages,
    [property: JsonPropertyName("regions")] IReadOnlyList<string>? Regions
);

public record SourceInfo(
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("source_info_link")] string? SourceInfoLink,
    [property: JsonPropertyName("icon")] string? Icon
);