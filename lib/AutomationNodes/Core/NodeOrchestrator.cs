using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AutomationNodes.Core
{
    public class Clients
    {
        private Clients()
        {
        }

        public static Clients All => new Clients { IsAll = true };
        public static Clients WithIds(params string[] connectionIds) => new Clients { ConnectionIds = connectionIds };
        public string[] ConnectionIds { init; get; }
        public bool IsAll { init; get; }
    }

    public interface INodeOrchestrator
    {
        void OnConnect(string connectionId);
        T CreateWorld<T>(CancellationToken token, params object[] parameters) where T : IWorld;
        IClientNode CreateNamedNode(Type type, Clients clients, string name, params object[] parameters);
        IClientNode GetNamedNode(Clients clients, string name);
        IClientNode CreateNode(Type type, Clients clients, params object[] parameters);
        IClientNode CreateChildNode(Type type, Clients clients, IClientNode parent, params object[] parameters);
        void SetProperty(Clients clients, IClientNode node, string propertyName, string propertyValue);
        void SetProperties(Clients clients, IClientNode node, Dictionary<string, string> properties);
        void SetTransition(Clients clients, IClientNode node, Dictionary<string, string> transitionProperties, TimeSpan duration, bool destroyAfter = false);
        void AddKeyframe(Dictionary<string, string> keyframeProperties, string keyframeName, string keyframePercent);
    }

    public class NodeOrchestrator : INodeOrchestrator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IWorldTime worldTime;
        private readonly IHubMessenger hubMessenger;
        private readonly IConnectedClients connectedClients;

        public NodeOrchestrator(
            IServiceProvider serviceProvider,
            IWorldTime worldTime,
            IHubMessenger hubMessenger,
            IConnectedClients connectedClients)
        {
            this.serviceProvider = serviceProvider;
            this.worldTime = worldTime;
            this.hubMessenger = hubMessenger;
            this.connectedClients = connectedClients;
        }

        private Dictionary<Guid, INode> allNodes = new();
        private List<KeyFrame> allKeyframes = new();
        private Dictionary<string, Dictionary<string, Guid>> nameConnectionNodeIdMap = new();
        private IWorld world;

        public void OnConnect(string connectionId)
        {
            SendState(connectionId);
        }

        private void SendState(string connectionId)
        {
            CatchUpNodes(allNodes);

            hubMessenger.SendWorldMessage(connectionId, world.Id);
            foreach(var node in allNodes)
            {
                hubMessenger.SendCreationMessage(connectionId, node.Value.CreationMessage());
                foreach(var property in node.Value.Properties)
                {
                    hubMessenger.SendSetPropertyMessage(connectionId, node.Value.Id, property.Key, property.Value);
                }
                if (node.Value.TransitionProperties != null)
                {
                    var duration = node.Value.TransitionEndTime - worldTime.Time.Elapsed;
                    if (duration > TimeSpan.Zero)
                    {
                        hubMessenger.SendTransitionMessage(connectionId, node.Value.Id, node.Value.TransitionProperties, duration, node.Value.DestroyAfterTransition);
                    }
                }
            }
        }

        private void CatchUpNodes(Dictionary<Guid, INode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Value.TransitionProperties != null)
                {
                    var durationRemaining = node.Value.TransitionEndTime - worldTime.Time.Elapsed;
                    if (durationRemaining <= TimeSpan.Zero)
                    {
                        if (node.Value.DestroyAfterTransition)
                        {
                            nodes.Remove(node.Key);
                        }
                        else
                        {
                            foreach (var property in node.Value.TransitionProperties)
                            {
                                node.Value.Properties[property.Key] = property.Value;
                            }
                        }
                    }
                    else
                    {
                        // TODO: partial transition
                    }
                }
            }
        }

        public T CreateWorld<T>(CancellationToken token, params object[] parameters) where T : IWorld
        {
            world = ConstructNode(typeof(T), null) as IWorld;
            world.CancellationToken = token;

            foreach(var client in connectedClients.ClientContexts)
            {
                hubMessenger.SendWorldMessage(client.Value.ConnectionId, world.Id);
            }

            allNodes[world.Id] = world as INode;
            world.OnCreated(Clients.All, parameters);

            return (T)world;
        }

        public IClientNode CreateNamedNode(Type type, Clients clients, string name, params object[] parameters)
        {
            return DoCreateNode(type, clients, name, null, parameters);
        }

        public IClientNode GetNamedNode(Clients clients, string name)
        {
            if (nameConnectionNodeIdMap.TryGetValue(name, out var connectionNodeIdMap))
            {
                if (clients.IsAll)
                {
                    CatchUpNodes(allNodes);
                    if (connectionNodeIdMap.TryGetValue(string.Empty, out var nodeId))
                    {
                        if (allNodes.TryGetValue(nodeId, out var node))
                        {
                            return new ClientNode(node); 
                        }

                        return null;
                    }
                }
                else
                {
                    foreach(var connectionId in clients.ConnectionIds)
                    {
                        CatchUpNodes(connectedClients.ClientContexts[connectionId].Nodes);
                    }

                    var clientNodes = clients.ConnectionIds
                        .Select(connectionId =>
                        {
                            if (connectionNodeIdMap.TryGetValue(connectionId, out var nodeId))
                            {
                                if (connectedClients.ClientContexts.TryGetValue(connectionId, out var client))
                                {
                                    if (client.Nodes.TryGetValue(nodeId, out var node))
                                    {
                                        return node;
                                    }
                                }
                            }

                            return null;
                        })
                        .Where(node => node != null)
                        .ToList();
                    return clientNodes.Count > 0 ? new ClientNode(clientNodes) : null;
                }
            }

            return null;
        }

        public IClientNode CreateNode(Type type, Clients clients, params object[] parameters)
        {
            return DoCreateNode(type, clients, null, null, parameters);
        }

        public IClientNode CreateChildNode(Type type, Clients clients, IClientNode parent, params object[] parameters)
        {
            return DoCreateNode(type, clients, null, parent, parameters);
        }

        private IClientNode DoCreateNode(Type type, Clients clients, string name, IClientNode parent, params object[] parameters)
        {
            var clientNodes = new List<INode>();

            if (clients.IsAll)
            {
                var node = ConstructNode(type, parent?.Node.First());
                if (name != null)
                {
                    nameConnectionNodeIdMap[name] = new Dictionary<string, Guid> { { string.Empty, node.Id } };
                }
                allNodes.Add(node.Id, node);
                node.OnCreate(parameters);
                foreach (var client in connectedClients.ClientContexts)
                {
                    hubMessenger.SendCreationMessage(client.Value.ConnectionId, node.CreationMessage());
                }
                node.OnCreated(clients, parameters);
                clientNodes.Add(node);
            }
            else
            {
                var enumerator = parent?.Node.GetEnumerator();
                var connectionNodeIdMap = GetOrCreateNameConnectionNodeIdMap(name);
                foreach (string connectionId in clients.ConnectionIds)
                {
                    enumerator?.MoveNext();
                    var node = ConstructNode(type, enumerator?.Current);
                    if (name != null)
                    {
                        connectionNodeIdMap[connectionId] = node.Id;
                    }
                    connectedClients.ClientContexts[connectionId].Nodes.Add(node.Id, node);
                    node.OnCreate(parameters);
                    hubMessenger.SendCreationMessage(connectionId, node.CreationMessage());
                    node.OnCreated(Clients.WithIds(connectionId), parameters);
                    clientNodes.Add(node);
                }
            }

            return new ClientNode(clientNodes);
        }

        private Dictionary<string, Guid> GetOrCreateNameConnectionNodeIdMap(string name)
        {
            if (name == null) return null;

            if (nameConnectionNodeIdMap.TryGetValue(name, out var map))
            {
                return map;
            }

            var connectionNodeIdMap = new Dictionary<string, Guid>();
            nameConnectionNodeIdMap[name] = connectionNodeIdMap;

            return connectionNodeIdMap;
        }

        private INode ConstructNode(Type type, INode parent)
        {
            var o = serviceProvider.GetService(type);
            if (o == null) throw new Exception($"Could not resolve {type.Name}. Maybe it's not registered?");
            if (!(o is INode node)) throw new Exception($"{type.Name} must be type of INode");

            node.Parent = parent;

            return node;
        }

        public void SetProperty(Clients clients, IClientNode node, string propertyName, string propertyValue)
        {
            var list = node.Node.ToList();
            if (clients.IsAll)
            {
                var nodeId = list[0].Id;
                allNodes[nodeId].Properties[propertyName] = propertyValue;
                foreach (var client in connectedClients.ClientContexts)
                {
                    hubMessenger.SendSetPropertyMessage(client.Value.ConnectionId, nodeId, propertyName, propertyValue);
                }
            }
            else 
            {
                var nodeCount = 0;
                foreach (string connectionId in clients.ConnectionIds)
                {
                    var nodeId = list[nodeCount++].Id;
                    connectedClients.ClientContexts[connectionId].Nodes[nodeId].Properties[propertyName] = propertyValue;
                    hubMessenger.SendSetPropertyMessage(connectionId, nodeId, propertyName, propertyValue);
                }
            }
        }

        public void SetProperties(Clients clients, IClientNode node, Dictionary<string, string> properties)
        {
            foreach(var property in properties)
            {
                SetProperty(clients, node, property.Key, property.Value);
            }
        }

        public void SetTransition(Clients clients, IClientNode node, Dictionary<string, string> transitionProperties, TimeSpan duration, bool destroyAfter = false)
        {
            var list = node.Node.ToList();
            if (clients.IsAll)
            {
                var nodeId = list[0].Id;
                allNodes[nodeId].TransitionProperties = transitionProperties;
                allNodes[nodeId].TransitionEndTime = worldTime.Time.Elapsed + duration;
                allNodes[nodeId].DestroyAfterTransition = destroyAfter;
                foreach (var client in connectedClients.ClientContexts)
                {
                    hubMessenger.SendTransitionMessage(client.Value.ConnectionId, nodeId, transitionProperties, duration, destroyAfter);
                }
            }
            else
            {
                var nodeCount = 0;
                foreach (string connectionId in clients.ConnectionIds)
                {
                    var nodeId = list[nodeCount++].Id;
                    connectedClients.ClientContexts[connectionId].Nodes[nodeId].TransitionProperties = transitionProperties;
                    connectedClients.ClientContexts[connectionId].Nodes[nodeId].TransitionEndTime = worldTime.Time.Elapsed + duration;
                    connectedClients.ClientContexts[connectionId].Nodes[nodeId].DestroyAfterTransition = destroyAfter;
                    hubMessenger.SendTransitionMessage(connectionId, nodeId, transitionProperties, duration, destroyAfter);
                }
            }
        }

        public void AddKeyframe(Dictionary<string, string> keyframeProperties, string keyframeName, string keyframePercent)
        {
            allKeyframes.Add(new KeyFrame
            {
                Name = keyframeName,
                Properties = keyframeProperties,
                Percent = keyframePercent
            });
            foreach (var client in connectedClients.ClientContexts)
            {
                hubMessenger.SendAddKeyframeMessage(client.Value.ConnectionId, keyframeProperties, keyframeName, keyframePercent);
            }
        }
    }
}
