// using System.Net.WebSockets;
// using System.Text;
// using System.Collections.Concurrent;

// namespace automobile_backend.WebSockets
// {
//     public class WebSocketHandler
//     {
//         // Store connected admin sockets
//         private static readonly ConcurrentDictionary<string, WebSocket> _adminSockets = new();

//         public static void AddSocket(string adminId, WebSocket socket)
//         {
//             _adminSockets.TryAdd(adminId, socket);
//         }

//         public static async Task RemoveSocket(string adminId)
//         {
//             _adminSockets.TryRemove(adminId, out _);
//             await Task.CompletedTask;
//         }

//         // Broadcast message to all connected admins
//         public static async Task BroadcastMessageAsync(string message)
//         {
//             var buffer = Encoding.UTF8.GetBytes(message);
//             var segment = new ArraySegment<byte>(buffer);

//             foreach (var socket in _adminSockets.Values)
//             {
//                 if (socket.State == WebSocketState.Open)
//                 {
//                     await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
//                 }
//             }
//         }

//         internal static async Task SendMessageToAllAdminsAsync(object newUserDto)
//         {
//             throw new NotImplementedException();
//         }
//     }
// }


//wed krn eka 


using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace automobile_backend.WebSockets
{
    public static class WebSocketHandler
    {
        // Store connected admin sockets
        private static readonly ConcurrentDictionary<string, WebSocket> _adminSockets = new();

        // Add new admin socket
        public static void AddSocket(string adminId, WebSocket socket)
        {
            _adminSockets.TryAdd(adminId, socket);
        }

        // Remove admin socket
        public static async Task RemoveSocket(string adminId)
        {
            _adminSockets.TryRemove(adminId, out _);
            await Task.CompletedTask;
        }

        // Broadcast a raw string message
        public static async Task BroadcastMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            foreach (var socket in _adminSockets.Values)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        // Broadcast an object as JSON
        public static async Task BroadcastObjectAsync(object data)
        {
            var json = JsonSerializer.Serialize(data);
            await BroadcastMessageAsync(json);
        }

        // Optional: send to a specific admin only
        public static async Task SendToAdminAsync(string adminId, object data)
        {
            if (_adminSockets.TryGetValue(adminId, out var socket) && socket.State == WebSocketState.Open)
            {
                var json = JsonSerializer.Serialize(data);
                var buffer = Encoding.UTF8.GetBytes(json);
                var segment = new ArraySegment<byte>(buffer);
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}


//after add modifi
