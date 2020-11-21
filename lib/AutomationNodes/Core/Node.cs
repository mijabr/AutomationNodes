using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public interface INode
    {
        Guid Id { get; }
        string ConnectionId { get; set; }
        string Type { get; }
        INode Parent { get; set; }
        void OnCreate(object[] parameters);
        void OnCreated(object[] parameters);
        Dictionary<string, object> CreationMessage();
    }

    public abstract class Node : INode
    {
        public Guid Id { get; } = Guid.NewGuid();

        public string ConnectionId { get; set; }

        public abstract string Type { get; }

        public INode Parent { get; set; }

        public virtual void OnCreate(object[] parameters)
        {
        }

        public virtual void OnCreated(object[] parameters)
        {
        }

        public virtual Dictionary<string, object> CreationMessage() => new Dictionary<string, object>
        {
            { "message", "Create" },
            { "id", Id },
            { "type", Type },
            { "parentId", Parent?.Id }
        };
    }
}
