// See https://aka.ms/new-console-template for more information
using bf_bot;
using bf_bot.Strategies.Soccer;
using Microsoft.Extensions.Logging;


using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Trace)
        // .AddFilter("Default", LogLevel.Trace)
        .AddSimpleConsole( options => {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "dd/mm/yyyy hh:mm:ss ";
        });
});
ILogger logger = loggerFactory.CreateLogger<Program>();

var initializer = Utility.CreateInitializer();

var client = new BetfairClient(initializer, logger);

var bothTeamToScoreStrategy = new BothTeamToScore(client, logger);

await bothTeamToScoreStrategy.Start();

logger.LogInformation("Program terminated.");
