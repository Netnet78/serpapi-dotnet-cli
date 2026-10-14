using System.ComponentModel;
using SerpApi.Cli.Models;
using SerpApi.Cli.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SerpApi.Cli.Commands;

public sealed class SearchSettings : CommandSettings
{
  [CommandOption("-q|--query <QUERY>")]
  [Description("Primary search query string.")]
  public string Query { get; init; } = string.Empty;

  [CommandOption("-k|--api-key <KEY>")]
  [Description("SerpApi API key. Defaults to SERPAPI_API_KEY environment variable.")]
  public string? ApiKey { get; init; }

  [CommandOption("-p|--param <PARAM>")]
  [Description("Arbitrary SerpApi parameters (e.g. -p hl=en -p gl=us -p device=desktop)")]
  public IDictionary<string, string?> Parameters { get; init; } = new Dictionary<string, string?>();
}

public sealed class SearchCommand : AsyncCommand<SearchSettings>
{
  private readonly ISerpApiClient client;

  public SearchCommand(ISerpApiClient client)
  {
    this.client = client;
  }

  public override async Task<int> ExecuteAsync(
    CommandContext context,
    SearchSettings settings,
    CancellationToken cancellationToken)
  {
    try
    {
      string? apiKey = settings.ApiKey ?? Environment.GetEnvironmentVariable("SERPAPI_API_KEY");

      if (string.IsNullOrWhiteSpace(apiKey))
      {
        AnsiConsole.MarkupLine("[bold red]Error:[/] SerpApi key is missing. Set [yellow]SERPAPI_API_KEY[/]" +
          " or pass [yellow]--api-key[/].");
        return 1;
      }

      SearchParameters searchParams = new(settings.Parameters);
      if (!string.IsNullOrWhiteSpace(settings.Query))
      {
        searchParams.Query = settings.Query;
      }

      SerpApiResponse? response = null;

      await AnsiConsole.Status()
        .Spinner(Spinner.Known.Aesthetic)
        .StartAsync($"Querying SerpApi for '[green]{settings.Query}[/]'...", async _ =>
        {
          response = await client.SearchAsync(apiKey, searchParams, cancellationToken);
        });

      if (response == null)
      {
        AnsiConsole.MarkupLine("[bold red]For some reason, the response couldn't be fetched.[/]");
        return 1;
      }

      if (response.OrganicResults == null || response.OrganicResults.Count == 0)
      {
        AnsiConsole.MarkupLine("[yellow]No organic results returned.[/]");
        return 0;
      }

      Table table = new Table()
        .Border(TableBorder.Rounded)
        .Title($"[bold cyan]Google Search Results via SerpApi[/] (Took {response.Metadata?.TotalTimeTaken}s)")
        .AddColumn(new TableColumn("[grey]#[/]").Centered())
        .AddColumn(new TableColumn("[bold green]Title[/]"))
        .AddColumn(new TableColumn("[bold blue]Link[/]"));

      foreach (OrganicResult item in response.OrganicResults)
      {
        table.AddRow(
          $"[dim]{item.Position}[/]",
          Markup.Escape(item.Title ?? string.Empty),
          $"[link={item.Link}]{Markup.Escape(item.Link ?? string.Empty)}[/]"
        );
      }

      AnsiConsole.Write(table);
      return 0;
    }
    catch (SerpApiException ex)
    {
      AnsiConsole.MarkupLine($"[red]Error Code: {ex.StatusCode}[/]\n[red]SerpApiException:[/] {ex.ApiErrorMessage}");
      return 1;
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine($"[red]Exception Unhandled:[/] {ex.Message}");
      return 1;
    }
  }
}