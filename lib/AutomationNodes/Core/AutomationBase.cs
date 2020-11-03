using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public interface INode
    {
        void SetProperty(string name, string value);
        void SetTransition(Dictionary<string, string> transitionProperties, TimeSpan duration);
    }

    public abstract class AutomationBase : INode, ITemporalEventHandler
    {
        protected readonly IWorld world;

        public AutomationBase(IWorld world)
        {
            this.world = world;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public abstract string Type { get; }

        public AutomationBase Parent { get; set; }

        public void SetProperty(string name, string value)
        {
            world.SetProperty(name, value, Id);
        }

        public void SetTransition(Dictionary<string, string> transitionProperties, TimeSpan duration)
        {
            world.SetTransition(transitionProperties, duration, Id);
        }

        public T CreateNode<T>() where T : AutomationBase
        {
            return world.CreateChildNode<T>(this);
        }

        public virtual void OnCreated()
        {
        }

        public virtual void OnEvent(TemporalEvent t)
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
