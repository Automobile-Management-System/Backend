// using System.Net.WebSockets;
// using System.Text;
// using System.Text.Json; // Add this at the top if not there

// public class WebSocketNotificationService
// {
//     private readonly List<WebSocket> _adminSockets = new();

//     public void AddAdminSocket(WebSocket socket) => _adminSockets.Add(socket);
//     public void RemoveAdminSocket(WebSocket socket) => _adminSockets.Remove(socket);

//     // public async Task NotifyAdminsAsync(string message)
//     // {
//     //     var buffer = Encoding.UTF8.GetBytes(message);
//     //     foreach (var socket in _adminSockets)
//     //     {
//     //         if (socket.State == WebSocketState.Open)
//     //             await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
//     //     }
//     // }
    

// public async Task NotifyAdminsAsync(string type, string message, string? name = null, string? email = null)
// {
//     var payload = new
//     {
//         type,
//         message,
//         name,
//         email,
//         timestamp = DateTime.UtcNow
//     };

//     string json = JsonSerializer.Serialize(payload);

//     foreach (var socket in _adminSockets)
//     {
//         if (socket.State == WebSocketState.Open)
//         {
//             var bytes = Encoding.UTF8.GetBytes(json);
//             await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
//         }
//     }
// }

// }

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

public class WebSocketNotificationService
{
    // Thread-safe collection for admin sockets
    private readonly ConcurrentDictionary<string, WebSocket> _adminSockets = new();

    /// <summary>
    /// Adds a new admin WebSocket connection
    /// </summary>
    public void AddAdminSocket(WebSocket socket)
    {
        var id = Guid.NewGuid().ToString();
        _adminSockets.TryAdd(id, socket);
    }

    /// <summary>
    /// Removes an admin WebSocket connection
    /// </summary>
    public void RemoveAdminSocket(WebSocket socket)
    {
        var key = _adminSockets.FirstOrDefault(kvp => kvp.Value == socket).Key;
        if (key != null)
        {
            _adminSockets.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Broadcasts a structured message to all connected admin sockets
    /// </summary>
    /// <param name="type">Type of notification (e.g., "info", "error")</param>
    /// <param name="message">Main message</param>
    /// <param name="name">Optional user/admin name</param>
    /// <param name="email">Optional user/admin email</param>
    public async Task NotifyAdminsAsync(string type, string message, string? name = null, string? email = null)
    {
        var payload = new
        {
            type,
            message,
            name,
            email,
            timestamp = DateTime.UtcNow
        };

        string json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        var disconnectedSockets = new List<string>();

        foreach (var kvp in _adminSockets)
        {
            var socket = kvp.Value;

            if (socket.State != WebSocketState.Open)
            {
                disconnectedSockets.Add(kvp.Key);
                continue;
            }

            try
            {
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                // Mark socket for removal if sending fails
                disconnectedSockets.Add(kvp.Key);
            }
        }

        // Remove closed or failed sockets
        foreach (var key in disconnectedSockets)
        {
            _adminSockets.TryRemove(key, out _);
        }
    }
}
