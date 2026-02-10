using Csharp_game_server.Server;
using Csharp_game_server.Client;

namespace Csharp_game_server;

class Program
{
    static void Main(string[] args)
    {
        if (args is ["--client", ..])
        {
            var client = new Client.Client();
            client.StartClient();
        }
        else
        {
            var server = new WebsocketServer();
        
            server.InitWebsocketServer("localhost", 8080);
        
            do
            {
                var line = Console.ReadLine();
                if (line != "stop") 
                    continue;
            
                _ = server.CloseWebsocketServer();
                break;
            } while (true);
        }
    }
}