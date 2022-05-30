// See https://aka.ms/new-console-template for more information
using bf_bot;
using bf_bot.Strategies.Soccer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using bf_bot.Constants;
using bf_bot.Wallets;
using bf_bot.Wallets.Impl;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Nest;
using Elasticsearch.Net;
using bf_bot.Utils;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var runningMode = RunningMode.TEST;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        var elasticClient = createElasticClient(configuration);

        var composedUrl = createElasticConnectionUrlForSerilog(configuration);

        var esSerilogIndex = configuration.GetValue<string>("Elasticsearch:serilogIndex");

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(composedUrl))
                {
                    IndexFormat = esSerilogIndex,
                    ModifyConnectionSettings = (configuration) =>
                        configuration.ServerCertificateValidationCallback((o, certificate, arg3, arg4) => { return true; })
                })
            .CreateLogger();

        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

        var serviceProvider = new ServiceCollection().AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(logger);
        })
            .AddSingleton<AppGuid>()
            .AddScoped<IWallet, SimpleProgressionWallet>()
            .AddScoped<IClient, BetfairRestClient>()
            .AddScoped<IStrategy, BothTeamToScore>()
            .BuildServiceProvider();


        var client = serviceProvider.GetRequiredService<IClient>();
        client.Init(Utility.CreateInitializer(), elasticClient);

        var wallet = serviceProvider.GetRequiredService<IWallet>();
        wallet.Init(1000, 2, elasticClient);

        var strategy = serviceProvider.GetRequiredService<IStrategy>();
        strategy.Init(
            runningMode,
            serviceProvider.GetRequiredService<IClient>(),
            serviceProvider.GetRequiredService<IWallet>(),
            elasticClient
        );

        await strategy.Start();
    }

    public static ElasticClient createElasticClient(IConfigurationRoot configuration)
    {
        var elasticProto = configuration.GetValue<string>("Elasticsearch:protocol");
        var elasticUser = configuration.GetValue<string>("Elasticsearch:user");
        var elasticPassword = configuration.GetValue<string>("Elasticsearch:password");
        var elasticUrl = configuration.GetValue<string>("Elasticsearch:nodeUrl");
        var esAppIndex = configuration.GetValue<string>("Elasticsearch:appIndex");
        var esSerilogIndex = configuration.GetValue<string>("Elasticsearch:serilogIndex");

        var esAppSettings = new ConnectionSettings(new Uri(elasticProto + "://" + elasticUrl))
            .DefaultIndex(esAppIndex)
            .BasicAuthentication(elasticUser, elasticPassword)
            .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
        var elasticClient = new ElasticClient(esAppSettings);

        return elasticClient;
    }

    public static string createElasticConnectionUrlForSerilog(IConfigurationRoot configuration)
    {
        var elasticProto = configuration.GetValue<string>("Elasticsearch:protocol");
        var elasticUser = configuration.GetValue<string>("Elasticsearch:user");
        var elasticPassword = configuration.GetValue<string>("Elasticsearch:password");
        var elasticUrl = configuration.GetValue<string>("Elasticsearch:nodeUrl");

        var composedUrl = elasticProto + "://" + elasticUser + ":" + elasticPassword + "@" + elasticUrl;

        return composedUrl;
    }
}