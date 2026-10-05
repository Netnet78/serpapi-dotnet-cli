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
}