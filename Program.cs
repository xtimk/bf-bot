// See https://aka.ms/new-console-template for more information
using bf_bot;

// create initializer obj, where are stored all the necessary things to create the client.
var initializer = Utility.CreateInitializer();

BetfairClient client = new BetfairClient(initializer);

var loginResult = await client.Login();

if(loginResult.IsOk)
{
    Console.WriteLine("Successfully logged in.");
    Console.WriteLine("Response details: ");
    Console.WriteLine(Utility.PrettyJsonObject(loginResult));

    Console.WriteLine("\n\nAuth Token: " + client.AuthToken);
}
else
{
    Console.WriteLine("Error when logging in.");
    Console.WriteLine(Utility.PrettyJsonObject(loginResult));
}