using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors(p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()
);
app.UseWebSockets();

app.MapGet("/", () => "Hello World");

List<WebSocket> webSockets = [];

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            webSockets.Add(webSocket);
            Console.WriteLine($"Connection Created: {webSockets.Count} connections");
            await Echo(webSocket);
            Console.WriteLine("Connection Closed");
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }

});

app.Run();

async Task Echo(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    var receiveResult = await webSocket.ReceiveAsync(
        new ArraySegment<byte>(buffer), CancellationToken.None);

    while (!receiveResult.CloseStatus.HasValue)
    {
        Console.WriteLine(Encoding.ASCII.GetString(buffer));

        foreach (WebSocket socket in webSockets)
        {
            if (socket.State == WebSocketState.Closed) { continue; }

            await socket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);
        }

        receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    webSockets.Remove(webSocket);
    await webSocket.CloseAsync(
        receiveResult.CloseStatus.Value,
        receiveResult.CloseStatusDescription,
        CancellationToken.None);
}
