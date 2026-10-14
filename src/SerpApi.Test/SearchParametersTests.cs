using Shouldly;
using SerpApi.Cli.Models;

namespace SerpApi.Cli.Tests;

public class SearchParametersTests
{
  [Fact]
  public void SearchParameters_NormalizesAliasesCorrectly()
  {
    // Arrange: User passes verbose aliases
    var input = new Dictionary<string, string?>
    {
      ["host_language"] = "en",
      ["geolocation"] = "us",
      ["num"] = "20",
      ["safe"] = "active"
    };

    // Act
    var parameters = new SearchParameters(input)
    {
      Query = "developer jobs"
    };

    // Assert
    parameters.Engine.ShouldBe("google");
    parameters.Query.ShouldBe("developer jobs");
    parameters["hl"].ShouldBe("en");
    parameters["gl"].ShouldBe("us");
    parameters["num"].ShouldBe("20");
    parameters["safe"].ShouldBe("active");

    // Aliases should have been cleaned up
    parameters.ContainsKey("host_language").ShouldBeFalse();
    parameters.ContainsKey("geolocation").ShouldBeFalse();
  }

  [Fact]
  public void Set_NormalizesAliases_AfterConstruction()
  {
    var parameters = new SearchParameters();

    parameters.Set("query", "coffee");
    parameters.Set("HOST_LANGUAGE", "en");

    parameters.Query.ShouldBe("coffee");
    parameters.HostLanguage.ShouldBe("en");
    parameters.ContainsKey("query").ShouldBeFalse();
    parameters.ContainsKey("host_language").ShouldBeFalse();
  }

  [Fact]
  public void Enumeration_AlwaysIncludesDefaultEngine()
  {
    var parameters = new SearchParameters { Query = "coffee" };

    var sent = parameters.ToDictionary(p => p.Key, p => p.Value);

    sent["engine"].ShouldBe("google");
    sent["q"].ShouldBe("coffee");
    parameters.Count.ShouldBe(2);
  }

  [Fact]
  public void Engine_ClearedWithBlank_FallsBackToDefault_AndIsStillSent()
  {
    var parameters = new SearchParameters { Engine = "bing" };

    parameters.Engine = "  ";

    parameters.Engine.ShouldBe("google");
    parameters.ShouldContain(p => p.Key == "engine" && p.Value == "google");
  }

  [Fact]
  public void Enumeration_ContainsEngineExactlyOnce_WhenCustomEngineSet()
  {
    var parameters = new SearchParameters { Engine = "bing" };

    parameters.Count(p => p.Key == "engine").ShouldBe(1);
    parameters.Single(p => p.Key == "engine").Value.ShouldBe("bing");
  }

  [Fact]
  public void Constructor_CaseOnlyDuplicateKeys_DoesNotThrow_LastWins()
  {
    var input = new Dictionary<string, string?> { ["q"] = "a", ["Q"] = "b" };

    var parameters = Should.NotThrow(() => new SearchParameters(input));

    parameters.Query.ShouldBe("b");
    parameters.Count(p => string.Equals(p.Key, "q", StringComparison.OrdinalIgnoreCase)).ShouldBe(1);
  }

  [Fact]
  public void Set_BlankValue_RemovesParameter()
  {
    var parameters = new SearchParameters { HostLanguage = "en" };

    parameters.HostLanguage = null;

    parameters.ContainsKey("hl").ShouldBeFalse();
    parameters.ShouldNotContain(p => p.Key == "hl");
  }

  [Fact]
  public void Constructor_SkipsBlankValues()
  {
    var input = new Dictionary<string, string?> { ["num"] = "", ["safe"] = null };

    var parameters = new SearchParameters(input);

    parameters.ContainsKey("num").ShouldBeFalse();
    parameters.ContainsKey("safe").ShouldBeFalse();
  }
}