using System.Net;
using System.Text;
using Shouldly;
using SerpApi.Cli.Models;
using SerpApi.Cli.Services;

namespace SerpApi.Test;

public class SerpApiClientTests
{
    private sealed class MockHttpMessageHandler(
      string jsonResponse,
      HttpStatusCode statusCode = HttpStatusCode.OK)
      : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
            };

            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task SearchGoogleAsync_SerializesParametersAndDeserializesResponse()
    {
        const string sampleJson = """
        {
        "search_metadata": {
            "id": "search_123",
            "status": "Success",
            "total_time_taken": 1.45,
            "json_endpoint": "https://serpapi.com/searches/123.json",
            "created_at": "2026-10-05",
            "processed_at": "2026-10-05",
            "google_url": "https://google.com/search?q=coffee",
            "raw_html_file": "https://serpapi.com/searches/123.html"
        },
        "organic_results": [
            {
            "position": 1,
            "title": "Best Coffee Near Me",
            "link": "https://example.com/coffee",
            "snippet": "Find the top coffee shops..."
            }
        ]
        }
        """;

        MockHttpMessageHandler handler = new(sampleJson);
        HttpClient httpClient = new(handler);
        SerpApiClient serpApiClient = new(httpClient);

        SearchParameters parameters = new()
        {
            Query = "coffee",
            HostLanguage = "en",
            Geolocation = "us"
        };

        // Act
        SerpApiResponse? response = await serpApiClient.SearchAsync("test_api_key", parameters);

        // Assert: Check query parameters sent in the URL
        Uri uri = handler.CapturedRequest!.RequestUri!;
        uri.Query.ShouldContain("api_key=test_api_key");
        uri.Query.ShouldContain("q=coffee");
        uri.Query.ShouldContain("hl=en");
        uri.Query.ShouldContain("gl=us");
        // Assert: Check deserialized response object
        response.ShouldNotBeNull();
        response.Metadata!.Id.ShouldBe("search_123");
        response.OrganicResults!.Count.ShouldBe(1);
        response.OrganicResults[0].Title.ShouldBe("Best Coffee Near Me");
    }
}
