// See https://aka.ms/new-console-template for more information
using bf_bot;

// Console.WriteLine("Hello, World!");

AppSettings app = AppSettings.Instance;

// Console.WriteLine(app.Username);

BetfairClient client = new BetfairClient();
await client.Login();