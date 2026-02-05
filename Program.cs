namespace Csharp_game_server;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var server = new WebsocketServer();
        
        server.InitWebsocketServer("localhost", 8080);
        
        do
        {
            var line = Console.ReadLine();
            if (line != "stop") 
                continue;
            
            server.CloseWebsocketServer();
            break;
        } while (true);
    }
}