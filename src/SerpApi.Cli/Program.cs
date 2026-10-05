using Microsoft.Extensions.DependencyInjection;
using SerpApi.Cli.Commands;
using SerpApi.Cli.Infrastructure;
using SerpApi.Cli.Services;
using Spectre.Console.Cli;

namespace SerpApi.Cli;

public static class Program
{
  public static async Task<int> Main(string[] args)
  {
    DotNetEnv.Env.Load();

    ServiceCollection services = new();
    services.AddHttpClient<ISerpApiClient, SerpApiClient>();

    TypeRegistrar registrar = new(services);
    CommandApp<SearchCommand> app = new(registrar);
    return await app.RunAsync(args);
  }
}
