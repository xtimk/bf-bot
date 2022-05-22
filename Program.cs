// See https://aka.ms/new-console-template for more information
using bf_bot;
using bf_bot.Strategies.Soccer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using bf_bot.Constants;

var runningMode = RunningMode.TEST;

var serviceProvider = new ServiceCollection().AddLogging(builder => {
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole( options => {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "[dd/mm/yyyy hh:mm:ss] ";
        });
    })
    .AddSingleton<BetfairClientInitializer>()
    .AddScoped<IClient, BetfairRestClient>(provider => new BetfairRestClient(provider.GetRequiredService<ILoggerFactory>(), Utility.CreateInitializer()))
    .AddScoped<IStrategy, BothTeamToScore>(provider => new BothTeamToScore(provider.GetRequiredService<IClient>(), provider.GetRequiredService<ILoggerFactory>(), runningMode))
    .BuildServiceProvider();

var btScoreStrategy = serviceProvider.GetService<IStrategy>();
await btScoreStrategy.Start();
