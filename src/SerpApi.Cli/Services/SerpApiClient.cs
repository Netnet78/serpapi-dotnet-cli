using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using SerpApi.Cli.Models;

namespace SerpApi.Cli.Services;

/// <summary>
/// Client implementation for interacting with the SerpApi search endpoint.
/// </summary>
public interface ISerpApiClient
{
  Task<SerpApiResponse?> SearchAsync(string apiKey, SearchParameters parameters, CancellationToken token = default);
}

/// <summary>
/// Represents error that occur during the api request
/// </summary>
public class SerpApiException : Exception
{
  public HttpStatusCode? StatusCode { get; }
  public string? ApiErrorMessage { get; }

  /// <summary>
  /// Initializes a new instance of the <c><see cref="SerpApiException"/></c> class with a specified error message,
  /// optional HTTP status code, API error details, and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The human-readable message that describes the error.</param>
  /// <param name="statusCode">The HTTP status code returned by the SerpApi endpoint, if applicable.</param>
  /// <param name="errorMessage">The raw error message string returned in the SerpApi JSON response payload, if available.</param>
  /// <param name="inner">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
  public SerpApiException(
    string message,
    HttpStatusCode? statusCode = null,
    string? errorMessage = null,
    Exception? inner = null) : base(message, inner)
  {
    StatusCode = statusCode;
    ApiErrorMessage = errorMessage;
  }
}

/// <summary>
/// Client implementation for interacting with SerpApi search endpoint.
/// </summary>
public class SerpApiClient : ISerpApiClient
{
  private readonly HttpClient httpClient;
  private const string BaseUrl = "https://serpapi.com/search.json";

  /// <summary>
  /// Initializes a new instance of the <see cref="SerpApiClient"/> class.
  /// </summary>
  /// <param name="http">The HTTP client instance used for requests.</param>
  public SerpApiClient(HttpClient http)
  {
    httpClient = http;
  }

  /// <summary>
  /// Executes a search query on Google using SerpApi.
  /// </summary>
  /// <param name="apiKey">The SerpApi API key.</param>
  /// <param name="parameters">The search parameters record.</param>
  /// <param name="token">A cancellation token to cancel the operation.</param>
  /// <returns>The deserialized <see cref="SerpApiResponse"/>, or <see langword="null"/>.</returns>
  /// <exception cref="SerpApiException">Thrown when the API returns an error or non-success status code.</exception>
  public async Task<SerpApiResponse?> SearchAsync(
    string apiKey,
    SearchParameters parameters,
    CancellationToken token = default)
  {
    UriBuilder builder = new(BaseUrl);
    // Initialize query params object
    NameValueCollection queryParams = HttpUtility.ParseQueryString(string.Empty);

    // Set every property to the query params
    queryParams["api_key"] = apiKey;
    // SearchParameters guarantees non-blank values and always includes the engine
    foreach (var (key, value) in parameters)
    {
      queryParams[key] = value;
    }

    builder.Query = queryParams.ToString();
    using var response = await httpClient.GetAsync(builder.Uri, token);
    string content = await response.Content.ReadAsStringAsync(token);

    if (!response.IsSuccessStatusCode)
    {
      string? apiError = TryExtractErrorFromJson(content);
      string message = apiError ?? "HTTP Error";
      throw new SerpApiException(
        $"SerpApi request failed with {response.StatusCode} : {message}",
        response.StatusCode,
        message);
    }

    // Handle potential SerpApi error payload returned with HTTP 200
    string? errorInSuccess = TryExtractErrorFromJson(content);
    if (!string.IsNullOrEmpty(errorInSuccess))
    {
      throw new SerpApiException(
          $"SerpApi returned an error: {errorInSuccess}",
          response.StatusCode,
          errorInSuccess);
    }

    return JsonSerializer.Deserialize<SerpApiResponse>(content);
  }

  /// <summary>
  /// Attepmts to extract the <c>error</c> from a JSON response.
  /// </summary>
  /// <param name="json">The JSON string to parse.</param>
  /// <returns>The error string if found; otherwise, <c>null</c>.</returns>
  public static string? TryExtractErrorFromJson(string json)
  {
    try
    {
      JsonNode? node = JsonNode.Parse(json);
      return node?["error"]?.GetValue<string>();
    }
    catch (Exception)
    {
      return null;
    }
  }
}