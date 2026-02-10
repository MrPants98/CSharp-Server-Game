using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;

namespace Csharp_game_server.Server;

public class WebsocketServer
{
    
    private readonly ConcurrentDictionary<Guid, WebSocket> _connectedClients = new();
    
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cts = new();

    public async Task InitWebsocketServer(string ip, int port) {
        if (!HttpListener.IsSupported) {
            Console.WriteLine("Needs Windows XP SP2, Server 2003 or later.");
            return;
        }

        var uri = $"http://{ip}:{port}/";
        _listener.Prefixes.Add(uri);
        
        _listener.Start();
        Console.WriteLine($"Listening on {uri}");

        while (!_cts.IsCancellationRequested) {
            var context = await _listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                _ = ProcessClientJoining(context);
            }
            else {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    public async Task CloseWebsocketServer()
    {
        await _cts.CancelAsync();
        
        foreach (var client in _connectedClients)
        {
            await client.Value.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server Closing", CancellationToken.None);
            client.Value.Dispose();
        }
        _listener.Stop();
        _listener.Close();
    }

    private async Task ProcessClientJoining(HttpListenerContext context)
    {

        WebSocket? socket = null;
        var clientId = Guid.NewGuid();

        try
        {
            var socketContext = await context.AcceptWebSocketAsync(null);
            socket = socketContext.WebSocket;
            _connectedClients[clientId] = socket;
            Console.WriteLine($"Client {clientId} Connected at ip {context.Request.RemoteEndPoint.Address.ToString()}");

            await ReceiveLoop(socket);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Client error: {e.Message}");
        }
        finally
        {
            if (socket != null)
            {
                _connectedClients.TryRemove(clientId, out _);

                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect", CancellationToken.None);
                
                socket.Dispose();
                Console.WriteLine($"Client {clientId} disconnected");
            }
        }
    }

    private async Task ReceiveLoop(WebSocket socket)
    {
        var buffer = new byte[512];
        var fullPacket =  new List<byte>();

        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                fullPacket.AddRange(buffer.Take(result.Count));
            } while (!result.EndOfMessage);
            
            PacketHandler.ReceivePacket(fullPacket.ToArray());
            
            await socket.SendAsync(new ArraySegment<byte>(fullPacket.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
            
            fullPacket.Clear();
        }
    }
}