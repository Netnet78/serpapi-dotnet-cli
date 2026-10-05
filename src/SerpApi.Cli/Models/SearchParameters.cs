namespace SerpApi.Cli.Models;

public class SearchParameters : Dictionary<string, string?>
{
  public SearchParameters() : base(StringComparer.OrdinalIgnoreCase)
  {
    this["engine"] = "google";
  }

  public SearchParameters(IDictionary<string, string?> dictionary)
    : base(dictionary, StringComparer.OrdinalIgnoreCase)
  {
    if (!ContainsKey("engine"))
    {
      this["engine"] = "google";
    }
    NormalizeAliases();
  }

  /// <summary>
  /// Primary search engine (defaults to "google").
  /// </summary>
  public string Engine
  {
    get => TryGetValue("engine", out var val) && !string.IsNullOrWhiteSpace(val) ? val : "google";
    set => this["engine"] = value;
  }

  /// <summary>
  /// Primary search query parameter ('q').
  /// </summary>
  public string? Query
  {
    get => TryGetValue("q", out var val) ? val : null;
    set => this["q"] = value;
  }

  /// <summary>
  /// Search location.
  /// </summary>
  public string? Location
  {
    get => TryGetValue("location", out var val) ? val : null;
    set => this["location"] = value;
  }

  /// <summary>
  /// Device used for search ('desktop', 'tablet', or 'mobile').
  /// </summary>
  public string? Device
  {
    get => TryGetValue("device", out var val) ? val : null;
    set => this["device"] = value;
  }

  /// <summary>
  /// Host language for Google search ('hl').
  /// </summary>
  public string? HostLanguage
  {
    get => TryGetValue("hl", out var val) ? val : null;
    set => this["hl"] = value;
  }

  /// <summary>
  /// Country / Geolocation code ('gl').
  /// </summary>
  public string? Geolocation
  {
    get => TryGetValue("gl", out var val) ? val : null;
    set => this["gl"] = value;
  }

  private void NormalizeAliases()
  {
    // Map common verbose or alternate names to query keys
    void MapAlias(string alias, string targetKey)
    {
      if (TryGetValue(alias, out var val) && !string.IsNullOrWhiteSpace(val))
      {
        this[targetKey] = val;
        Remove(alias);
      }
    }

    MapAlias("query", "q");
    MapAlias("hostlanguage", "hl");
    MapAlias("host_language", "hl");
    MapAlias("geolocation", "gl");
    MapAlias("geo_location", "gl");
  }
}