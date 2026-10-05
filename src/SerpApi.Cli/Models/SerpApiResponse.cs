using System.Text.Json.Serialization;

namespace SerpApi.Cli.Models;

public record SerpApiResponse(
  [property: JsonPropertyName("search_metadata")] SearchMetadata? Metadata,
  [property: JsonPropertyName("organic_results")] List<OrganicResult>? OrganicResults
);