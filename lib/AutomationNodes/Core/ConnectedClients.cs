using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public class ClientContext
    {
        public string ConnectionId { get; set; }
        public Caps Caps { get; set; }
        public double ImageScaling { get; set; } = 1.0;
        public double FontScaling { get; set; } = 1.0;
        public string ScaledImage(double size) => $"{size * ImageScaling}%";
        public string ScaledFont(double size) => $"{size * FontScaling}em";
        public Dictionary<Guid, INode> Nodes { get; } = new();
    }

    public interface IConnectedClients
    {
        Dictionary<string, ClientContext> ClientContexts { get; }
        void Connect(string connectionId, Caps caps);
        void Disconnect(string connectionId);
    }

    public class ConnectedClients : IConnectedClients
    {
        public void Connect(string connectionId, Caps caps)
        {
            var client = new ClientContext
            {
                ConnectionId = connectionId,
                Caps = caps
            };

            if (client.Caps.isMobile)
            {
                client.ImageScaling = 2.0;
                client.FontScaling = 1.3;
            }

            ClientContexts.Add(connectionId, client);
        }

        public void Disconnect(string connectionId)
        {
            ClientContexts.Remove(connectionId);
        }

        public Dictionary<string, ClientContext> ClientContexts { get; } = new();
    }
}
