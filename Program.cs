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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();
        
        var runningMode = readRunningMode(configuration);

        var elasticClient = createElasticClient(configuration);

        var composedUrl = createElasticConnectionUrlForSerilog(configuration);

        var esSerilogIndex = configuration.GetValue<string>("Elasticsearch:serilogIndex");

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            // I put elasticsearch here instead of in the appsettings.json 
            // because i need to override the ServerCertificateValidationCallback
            // in order to trust self-signed certs.
            .WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(composedUrl))
                {
                    IndexFormat = esSerilogIndex,
                    ModifyConnectionSettings = configuration =>
                        configuration.ServerCertificateValidationCallback((o, certificate, arg3, arg4) => { return true; })
                })
            .CreateLogger();
        // Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

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

        var strategy = serviceProvider.GetRequiredService<IStrategy>();

        strategy.Init(
            runningMode,
            serviceProvider.GetRequiredService<IClient>(),
            serviceProvider.GetRequiredService<IWallet>(),
            elasticClient
        );

        InitializeWallet(wallet, runningMode, elasticClient);
        
        await strategy.Start();
    }

    public static void InitializeWallet(IWallet wallet, RunningMode runningMode, ElasticClient elasticClient)
    {
        if(runningMode == RunningMode.TEST)
        {
            var balance = 1000;
            var win_per_cycle = 2;
            wallet.Init(balance, win_per_cycle, elasticClient);
        }
        else if (runningMode == RunningMode.REAL)
        {
            throw new NotImplementedException("Method to retrieve account balance not implemented.");
        }
    }

    public static RunningMode readRunningMode(IConfigurationRoot configuration)
    {
        var runningMode_s = configuration.GetValue<string>("RunningMode");
        var runningMode = runningMode_s switch
        {
            "TEST" => RunningMode.TEST,
            "REAL" => RunningMode.REAL,
            _ => throw new ArgumentException("The running mode specified (" + runningMode_s + ") is not valid. Valid values are TEST, REAL")
        };
        return runningMode;
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