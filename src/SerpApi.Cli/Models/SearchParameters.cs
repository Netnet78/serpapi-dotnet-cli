using System.Collections;

namespace SerpApi.Cli.Models;

/// <summary>
/// SerpApi query parameters. Wraps a private case-insensitive dictionary so that
/// every mutation goes through <see cref="Set"/>, guaranteeing that aliases are
/// normalized and blank values are dropped. Enumerating yields exactly what will be sent.
/// </summary>
public sealed class SearchParameters : IEnumerable<KeyValuePair<string, string>>
{
  private const string DefaultEngine = "google";

  // Map common verbose or alternate names to query keys
  private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
  {
    ["query"] = "q",
    ["hostlanguage"] = "hl",
    ["host_language"] = "hl",
    ["geolocation"] = "gl",
    ["geo_location"] = "gl",
  };

  private readonly Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);

  public SearchParameters()
  {
  }

  /// <summary>
  /// Creates parameters from arbitrary key/value pairs. Aliases are normalized and
  /// duplicate keys (including case-only duplicates) resolve as last-wins.
  /// </summary>
  public SearchParameters(IEnumerable<KeyValuePair<string, string?>> input)
  {
    foreach (var (key, value) in input)
    {
      Set(key, value);
    }
  }

  /// <summary>
  /// Gets the stored value for a canonical key (e.g. "q", "hl"), or <see langword="null"/>.
  /// Aliases are not resolved on read because they are never stored.
  /// </summary>
  public string? this[string key] => values.TryGetValue(key, out var val) ? val : null;

  /// <summary>
  /// Number of parameters that will be sent (including the default engine).
  /// </summary>
  public int Count => values.ContainsKey("engine") ? values.Count : values.Count + 1;

  /// <summary>
  /// Primary search engine (defaults to "google").
  /// </summary>
  public string Engine
  {
    get => this["engine"] ?? DefaultEngine;
    set => Set("engine", value);
  }

  /// <summary>
  /// Primary search query parameter ('q').
  /// </summary>
  public string? Query
  {
    get => this["q"];
    set => Set("q", value);
  }

  /// <summary>
  /// Search location.
  /// </summary>
  public string? Location
  {
    get => this["location"];
    set => Set("location", value);
  }

  /// <summary>
  /// Device used for search ('desktop', 'tablet', or 'mobile').
  /// </summary>
  public string? Device
  {
    get => this["device"];
    set => Set("device", value);
  }

  /// <summary>
  /// Host language for Google search ('hl').
  /// </summary>
  public string? HostLanguage
  {
    get => this["hl"];
    set => Set("hl", value);
  }

  /// <summary>
  /// Country / Geolocation code ('gl').
  /// </summary>
  public string? Geolocation
  {
    get => this["gl"];
    set => Set("gl", value);
  }

  /// <summary>
  /// The single mutation point. Normalizes aliases to their canonical key;
  /// a <see langword="null"/> or whitespace value removes the parameter.
  /// </summary>
  public void Set(string key, string? value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(key);

    string canonical = Aliases.TryGetValue(key, out var mapped) ? mapped : key;
    if (string.IsNullOrWhiteSpace(value))
    {
      values.Remove(canonical);
    }
    else
    {
      values[canonical] = value;
    }
  }

  /// <summary>
  /// Whether a canonical key is stored. Aliases are not resolved.
  /// </summary>
  public bool ContainsKey(string key) => values.ContainsKey(key);

  /// <summary>
  /// Yields every parameter to send, always including <c>engine</c>.
  /// </summary>
  public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
  {
    if (!values.ContainsKey("engine"))
    {
      yield return new("engine", DefaultEngine);
    }

    foreach (var pair in values)
    {
      yield return pair;
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}