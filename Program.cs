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

var runningMode = RunningMode.TEST;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .Build();

var elasticUrl = configuration.GetValue<string>("ElasticSearchUrl:nodeUrl");

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Elasticsearch(
        new ElasticsearchSinkOptions(new Uri(elasticUrl))
        {
            IndexFormat = "bf-bot-system-logs-{0:yyyy.MM.dd}",
            ModifyConnectionSettings = (configuration) => 
                configuration.ServerCertificateValidationCallback((o, certificate, arg3, arg4) => { return true; })
        })
    .CreateLogger();

Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
        
var serviceProvider = new ServiceCollection().AddLogging(builder => {
    builder.ClearProviders();
    builder.AddSerilog(logger);
    })
    .AddScoped<IWallet, SimpleProgressionWallet>()
    .AddSingleton<BetfairClientInitializer>()
    .AddScoped<IClient, BetfairRestClient>()
    .AddScoped<IStrategy, BothTeamToScore>()
    .BuildServiceProvider();


var client = serviceProvider.GetRequiredService<IClient>();
client.Init(Utility.CreateInitializer());

var wallet = serviceProvider.GetRequiredService<IWallet>();
wallet.Init(1000, 2);

var strategy = serviceProvider.GetRequiredService<IStrategy>();
strategy.Init(
    runningMode,
    serviceProvider.GetRequiredService<IClient>(),
    serviceProvider.GetRequiredService<IWallet>()
);

await strategy.Start();
