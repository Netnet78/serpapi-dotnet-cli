using System.Net;
using System.Text;
using System.Text.Json;
using Shouldly;
using SerpApi.Cli.Models;
using SerpApi.Cli.Services;

namespace SerpApi.Test;

/// <summary>
/// Failure-path tests for <see cref="SerpApiClient"/>. A stub handler lets each test
/// decide exactly what the "server" does: return an error status, a bad body, or throw.
/// </summary>
public class SerpApiClientFailureTests
{
  private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> respond)
    : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      => Task.FromResult(respond(request, cancellationToken));

    public static StubHandler Returning(HttpStatusCode status, string body, string mediaType = "application/json")
      => new((_, _) => new HttpResponseMessage(status)
      {
        Content = new StringContent(body, Encoding.UTF8, mediaType),
      });

    public static StubHandler Throwing(Exception exception)
      => new((_, _) => throw exception);
  }

  private static SerpApiClient CreateClient(HttpMessageHandler handler) => new(new HttpClient(handler));

  private static SearchParameters AnyParameters() => new() { Query = "coffee" };

  [Fact]
  public async Task NonSuccessStatus_WithJsonError_ThrowsSerpApiExceptionWithDetails()
  {
    var client = CreateClient(StubHandler.Returning(HttpStatusCode.Unauthorized, """{ "error": "Invalid API key." }"""));

    var ex = await Should.ThrowAsync<SerpApiException>(() => client.SearchAsync("bad_key", AnyParameters()));

    ex.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    ex.ApiErrorMessage.ShouldBe("Invalid API key.");
    ex.Message.ShouldBe("SerpApi request failed with 401 : Invalid API key.");
  }

  [Fact]
  public async Task NonSuccessStatus_WithNonJsonBody_FallsBackToGenericMessage()
  {
    var client = CreateClient(StubHandler.Returning(HttpStatusCode.BadGateway, "<html>502 Bad Gateway</html>", "text/html"));

    var ex = await Should.ThrowAsync<SerpApiException>(() => client.SearchAsync("key", AnyParameters()));

    ex.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
    ex.ApiErrorMessage.ShouldBe("HTTP Error");
  }

  [Fact]
  public async Task SuccessStatus_WithErrorPayload_ThrowsSerpApiException()
  {
    // SerpApi sometimes reports errors with HTTP 200
    var client = CreateClient(StubHandler.Returning(HttpStatusCode.OK, """{ "error": "Your account has run out of searches." }"""));

    var ex = await Should.ThrowAsync<SerpApiException>(() => client.SearchAsync("key", AnyParameters()));

    ex.StatusCode.ShouldBe(HttpStatusCode.OK);
    ex.ApiErrorMessage.ShouldBe("Your account has run out of searches.");
  }

  [Fact]
  public async Task SuccessStatus_WithMalformedJson_ThrowsJsonException()
  {
    // Documents current behavior: malformed bodies are NOT wrapped in SerpApiException
    var client = CreateClient(StubHandler.Returning(HttpStatusCode.OK, "{ not valid json"));

    await Assert.ThrowsAnyAsync<JsonException>(() => client.SearchAsync("key", AnyParameters()));
  }

  [Fact]
  public async Task NetworkFailure_PropagatesHttpRequestException()
  {
    // Documents current behavior: transport errors are NOT wrapped in SerpApiException
    var client = CreateClient(StubHandler.Throwing(new HttpRequestException("No such host is known.")));

    await Should.ThrowAsync<HttpRequestException>(() => client.SearchAsync("key", AnyParameters()));
  }

  [Fact]
  public async Task CancelledToken_ThrowsOperationCanceled()
  {
    var client = CreateClient(new StubHandler((_, token) =>
    {
      token.ThrowIfCancellationRequested();
      return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
    }));
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    // ThrowsAnyAsync accepts derived types (HttpClient may surface TaskCanceledException)
    await Assert.ThrowsAnyAsync<OperationCanceledException>(
      () => client.SearchAsync("key", AnyParameters(), cts.Token));
  }

  [Fact]
  public async Task SuccessStatus_WithNoOrganicResults_ReturnsResponseWithNullList()
  {
    var client = CreateClient(StubHandler.Returning(HttpStatusCode.OK, """{ "search_metadata": { "id": "x" } }"""));

    var response = await client.SearchAsync("key", AnyParameters());

    response.ShouldNotBeNull();
    response.OrganicResults.ShouldBeNull();
  }
}
