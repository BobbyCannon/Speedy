using System.Net;
using Speedy.Net;

try
{
	var proxy = new ProxyServer(IPAddress.Parse("10.0.0.3"), 8080);
	proxy.StartServer();
}
catch (Exception ex)
{
	Console.WriteLine($"{DateTime.Now}  {ex.Message}");
	Console.WriteLine("Press Enter to close application");
	Console.ReadLine();
	Environment.Exit(1);
}