// See https://aka.ms/new-console-template for more information
using bf_bot;
using bf_bot.Strategies.Soccer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection().AddLogging(builder => {
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddSimpleConsole( options => {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "dd/mm/yyyy hh:mm:ss ";
        });
    })
    .AddSingleton<BetfairClientInitializer>()
    .AddScoped<IClient, BetfairClient>(provider => new BetfairClient(provider.GetRequiredService<ILoggerFactory>(), Utility.CreateInitializer()))
    .AddScoped<IStrategy, BothTeamToScore>(provider => new BothTeamToScore(provider.GetRequiredService<IClient>(), provider.GetRequiredService<ILoggerFactory>()))
    .BuildServiceProvider();
    
// serviceProvider.GetService<ILoggerFactory>();

// var bfClient = serviceProvider.GetService<IClient>();

// serviceProvider.GetService<IClient>().Init(Utility.CreateInitializer());

// var initializer = Utility.CreateInitializer();
// bfClient.Init(initializer);

var btScoreStrategy = serviceProvider.GetService<IStrategy>();
await btScoreStrategy.Start();


// var client = new BetfairClient(initializer, loggerFactory);

// var bothTeamToScoreStrategy = new BothTeamToScore(client, logger);

// await bothTeamToScoreStrategy.Start();

// logger.LogInformation("Program terminated.");
