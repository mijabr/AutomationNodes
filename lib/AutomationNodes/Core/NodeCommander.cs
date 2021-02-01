using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public interface INodeCommander
    {
        T CreateNode<T>(string connectionId, params object[] parameters) where T : INode;
        object CreateNode(Type type, string connectionId, params object[] parameters);
        T CreateChildNode<T>(INode parent, params object[] parameters) where T : INode;
        object CreateChildNode(Type type, INode parent, params object[] parameters);
        T CreateWorld<T>(string connectionId) where T : INode;
        void SetProperty(INode node, string propertyName, string propertyValue);
        void SetProperties(INode node, Dictionary<string, string> properties);
        void SetTransition(INode node, Dictionary<string, string> transitionProperties, TimeSpan duration, bool destroyAfter = false);
        void AddKeyframe(string connectionId, Dictionary<string, string> keyframeProperties, string keyframeName, string keyframePercent);
    }

    public class NodeCommander : INodeCommander
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHubManager hubManager;

        public NodeCommander(
            IServiceProvider serviceProvider,
            IHubManager hubManager)
        {
            this.serviceProvider = serviceProvider;
            this.hubManager = hubManager;
        }

        public T CreateNode<T>(string connectionId, params object[] parameters) where T : INode
        {
            return (T)DoCreateNode(typeof(T), connectionId, null, parameters);
        }

        public object CreateNode(Type type, string connectionId, params object[] parameters)
        {
            return DoCreateNode(type, connectionId, null, parameters);
        }

        public T CreateChildNode<T>(INode parent, params object[] parameters) where T : INode
        {
            return (T)DoCreateNode(typeof(T), parent.ConnectionId, parent, parameters);
        }

        public object CreateChildNode(Type type, INode parent, params object[] parameters)
        {
            return DoCreateNode(type, parent.ConnectionId, parent, parameters);
        }

        public T CreateWorld<T>(string connectionId) where T : INode
        {
            var world = (T)ConstructNode(typeof(T), connectionId, null);

            hubManager.Send(connectionId, new Dictionary<string, object>
            {
                { "message", "World" },
                { "id", world.Id }
            });

            world.OnCreated(null);

            return world;
        }

        private object DoCreateNode(Type type, string connectionId, INode parent, params object[] parameters)
        {
            var node = ConstructNode(type, connectionId, parent, parameters);
            node.OnCreate(parameters);
            hubManager.Send(node.ConnectionId, node.CreationMessage());
            node.OnCreated(parameters);

            return node;
        }

        private INode ConstructNode(Type type, string connectionId, INode parent, params object[] parameters)
        {
            var o = serviceProvider.GetService(type);
            if (o == null) throw new Exception($"Could not create {type.Name}. Maybe it's not registered?");
            if (!(o is INode node)) throw new Exception($"{type.Name} must be type of INode");

            node.Parent = parent;
            node.ConnectionId = connectionId;

            return node;
        }

        public void SetProperty(INode node, string propertyName, string propertyValue)
        {
            hubManager.Send(node.ConnectionId, new Dictionary<string, object>
            {
                { "message", "SetProperty" },
                { "id", node.Id },
                { "name", propertyName },
                { "value", propertyValue }
            });
        }

        public void SetProperties(INode node, Dictionary<string, string> properties)
        {
            foreach(var property in properties)
            {
                SetProperty(node, property.Key, property.Value);
            }
        }

        public void SetTransition(INode node, Dictionary<string, string> transitionProperties, TimeSpan duration, bool destroyAfter = false)
        {
            hubManager.Send(node.ConnectionId, new Dictionary<string, object>
            {
                { "message", "SetTransition" },
                { "id", node.Id },
                { "properties", transitionProperties },
                { "duration", duration.TotalMilliseconds },
                { "destroyAfter", destroyAfter }
            });
        }

        public void AddKeyframe(string connectionId, Dictionary<string, string> keyframeProperties, string keyframeName, string keyframePercent)
        {
            hubManager.Send(connectionId, new Dictionary<string, object>
            {
                { "message", "AddKeyframe" },
                { "properties", keyframeProperties },
                { "keyframename", keyframeName },
                { "keyframepercent", keyframePercent }
            });
        }
    }
}
