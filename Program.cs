// See https://aka.ms/new-console-template for more information
using bf_bot;
using bf_bot.Strategies.Soccer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using bf_bot.Constants;
using bf_bot.Wallets;
using bf_bot.Wallets.Impl;

var runningMode = RunningMode.TEST;

var serviceProvider = new ServiceCollection().AddLogging(builder => {
    builder
        .SetMinimumLevel(LogLevel.Trace)
        .AddSimpleConsole( options => {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "[dd/mm/yyyy hh:mm:ss] ";
        });
    })
    .AddScoped<IWallet, SimpleProgressionWallet>()
    .AddSingleton<BetfairClientInitializer>()
    .AddScoped<IClient, BetfairRestClient>(provider => new BetfairRestClient(provider.GetRequiredService<ILoggerFactory>(), Utility.CreateInitializer()))
    .AddScoped<IStrategy, BothTeamToScore>(provider => 
        new BothTeamToScore(
                runningMode, 
                provider.GetRequiredService<IClient>(), 
                provider.GetRequiredService<ILoggerFactory>(), 
                provider.GetRequiredService<IWallet>()
            )
        )
    .BuildServiceProvider();

var btScoreStrategy = serviceProvider.GetService<IStrategy>();
await btScoreStrategy.Start();
