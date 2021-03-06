﻿using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public interface INodeCommander
    {
        T CreateNode<T>(string connectionId, params object[] parameters) where T : INode;
        object CreateNode(Type type, string connectionId, params object[] parameters);
        T CreateChildNode<T>(INode parent, params object[] parameters) where T : INode;
        object CreateChildNode(Type type, INode parent, params object[] parameters);
        T CreateWorld<T>(string connectionId, params object[] parameters) where T : INode;
        void SetProperty(INode node, string propertyName, string propertyValue);
        void SetProperties(INode node, Dictionary<string, string> properties);
        void SetTransition(INode node, Dictionary<string, string> transitionProperties, TimeSpan duration, bool destroyAfter = false);
        void AddKeyframe(string connectionId, Dictionary<string, string> keyframeProperties, string keyframeName, string keyframePercent);
    }

    public class NodeCommander : INodeCommander
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHubDownstream hubDownstream;

        public NodeCommander(
            IServiceProvider serviceProvider,
            IHubDownstream hubDownstream)
        {
            this.serviceProvider = serviceProvider;
            this.hubDownstream = hubDownstream;
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

        public T CreateWorld<T>(string connectionId, params object[] parameters) where T : INode
        {
            var world = (T)ConstructNode(typeof(T), connectionId, null);

            hubDownstream.Send(connectionId, new Dictionary<string, object>
            {
                { "message", "World" },
                { "id", world.Id }
            });

            world.OnCreated(Clients.All, parameters);

            return world;
        }

        private object DoCreateNode(Type type, string connectionId, INode parent, params object[] parameters)
        {
            var node = ConstructNode(type, connectionId, parent);
            node.OnCreate(parameters);
            hubDownstream.Send(node.ConnectionId, node.CreationMessage());
            node.OnCreated(Clients.All, parameters);

            return node;
        }

        private INode ConstructNode(Type type, string connectionId, INode parent)
        {
            var o = serviceProvider.GetService(type);
            if (o == null) throw new Exception($"Could not resolve {type.Name}. Maybe it's not registered?");
            if (!(o is INode node)) throw new Exception($"{type.Name} must be type of INode");

            node.Parent = parent;
            node.ConnectionId = connectionId;

            return node;
        }

        public void SetProperty(INode node, string propertyName, string propertyValue)
        {
            hubDownstream.Send(node.ConnectionId, new Dictionary<string, object>
            {
                { "message", "SetProperty" },
                { "id", node.Id },
                { "name", propertyName },
                { "value", propertyValue }
            });
        }

        public void SetProperties(INode node, Dictionary<string, string> properties)
        {
            foreach (var property in properties)
            {
                SetProperty(node, property.Key, property.Value);
            }
        }

        public void SetTransition(INode node, Dictionary<string, string> transitionProperties, TimeSpan duration, bool destroyAfter = false)
        {
            hubDownstream.Send(node.ConnectionId, new Dictionary<string, object>
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
            hubDownstream.Send(connectionId, new Dictionary<string, object>
            {
                { "message", "AddKeyframe" },
                { "properties", keyframeProperties },
                { "keyframename", keyframeName },
                { "keyframepercent", keyframePercent }
            });
        }
    }
}
