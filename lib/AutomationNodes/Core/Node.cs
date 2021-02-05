using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public interface IClientNode
    {
        IEnumerable<INode> Node { get; }
    }

    public class ClientNode : IClientNode
    {
        public ClientNode(INode node)
        {
            clientNodes = new List<INode> { node };
        }

        public ClientNode(List<INode> nodes)
        {
            clientNodes = nodes;
        }

        List<INode> clientNodes;

        public IEnumerable<INode> Node => clientNodes;
    }

    public interface INode
    {
        Guid Id { get; }
        string ConnectionId { get; set; }
        string Type { get; }
        Dictionary<string, string> Properties { get; }
        Dictionary<string, string> TransitionProperties { get; set; }
        TimeSpan TransitionEndTime { get; set; }
        bool DestroyAfterTransition { get; set; }
        INode Parent { get; set; }
        void OnCreate(object[] parameters);
        void OnCreated(Clients clients, object[] parameters);
        Dictionary<string, object> CreationMessage();
    }

    public abstract class Node : INode
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string ConnectionId { get; set; }

        public abstract string Type { get; }

        public Dictionary<string, string> Properties { get; } = new();

        public Dictionary<string, string> TransitionProperties { get; set; }

        public TimeSpan TransitionEndTime { get; set; }

        public bool DestroyAfterTransition { get; set; }

        public INode Parent { get; set; }

        public virtual void OnCreate(object[] parameters)
        {
        }

        public virtual void OnCreated(Clients clients, object[] parameters)
        {
        }

        public virtual Dictionary<string, object> CreationMessage() => new Dictionary<string, object>
        {
            { "id", Id },
            { "type", Type },
            { "parentId", Parent?.Id }
        };
    }
}
