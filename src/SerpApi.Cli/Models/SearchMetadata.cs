using System.Text.Json.Serialization;

namespace SerpApi.Cli.Models;

public record SearchMetadata(
  [property: JsonPropertyName("id")] string? Id,
  [property: JsonPropertyName("status")] string? Status,
  [property: JsonPropertyName("total_time_taken")] double? TotalTimeTaken,
  [property: JsonPropertyName("json_endpoint")] string? JsonEndpoint,
  [property: JsonPropertyName("created_at")] string? CreatedAt,
  [property: JsonPropertyName("processed_at")] string? ProcessedAt,
  [property: JsonPropertyName("google_url")] string? GoogleUrl,
  [property: JsonPropertyName("raw_html_file")] string? RawHtmlFile
);