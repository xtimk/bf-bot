// See https://aka.ms/new-console-template for more information
using bf_bot;


// AppSettings app = AppSettings.Instance;

BetfairClient client = new BetfairClient();
var loginResult = await client.Login();

if(loginResult.IsSuccessfull)
{
    Console.WriteLine("Successfully logged in.");
    Console.WriteLine("Response details: ");
    Console.WriteLine(Utility.PrettyJsonObject(loginResult));

    Console.WriteLine("\n\nAuth Token: " + client.AuthToken);
}