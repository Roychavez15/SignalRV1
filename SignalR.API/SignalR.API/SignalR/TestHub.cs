using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace SignalR.API.SignalR
{
    public class TestHub : Hub
    {
        private static Dictionary<int, string> deviceConnections;
        private static Dictionary<string, int> connectionDevices;
        private readonly ILogger<TestHub> _logger;

        public TestHub(ILogger<TestHub> logger)
        {
            deviceConnections = deviceConnections ?? new Dictionary<int, string>();
            connectionDevices = connectionDevices ?? new Dictionary<string, int>();
            _logger = logger;
        }
        public override Task OnConnectedAsync()
        {
            Debug.WriteLine("server conectado");
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            int? deviceId = connectionDevices.ContainsKey(Context.ConnectionId) ?
                (int?)connectionDevices[Context.ConnectionId] : null;
            if (deviceId.HasValue)
            {
                deviceConnections.Remove(deviceId.Value);
                connectionDevices.Remove(Context.ConnectionId);
            }
            Debug.WriteLine($"server desconectado. Device : {deviceId}.");
            return base.OnDisconnectedAsync(exception);
        }

        [HubMethodName("Init")]
        public Task Init(DeviceInfo info)
        {
            deviceConnections.Add(info.Id, Context.ConnectionId);
            connectionDevices.Add(Context.ConnectionId, info.Id);

            return Task.CompletedTask;
        }
        [HubMethodName("SendMessageToAll")]
        public async Task SendMessageToAll(MessageItem item)
        {
            _logger.LogInformation($"Se ha enviado mensaje desde el server {item.Message} desde {item.SourceId} para Todos");
            await Clients.All.SendAsync("NewMessage", item);
        }
        public async Task SendMessageToDevice(MessageItem item)
        {
            Debug.WriteLine($"Se ha enviado mensaje desde el server {item.Message} desde {item.SourceId} para {item.TargetId}");
            _logger.LogInformation($"Se ha enviado mensaje desde el server {item.Message} desde {item.SourceId} para {item.TargetId}");

            if (deviceConnections.ContainsKey(item.TargetId))
                await Clients.Client(deviceConnections[item.TargetId]).SendAsync("NewMessage", item);
        }


    }
}
