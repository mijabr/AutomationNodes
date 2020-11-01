using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public abstract class AutomationBase : ITemporalEventHandler
    {
        protected readonly WorldCatalogue worldCatalogue;
        protected readonly WorldBase world;

        public AutomationBase(WorldCatalogue worldCatalogue, WorldBase world)
        {
            this.worldCatalogue = worldCatalogue;
            this.world = world;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public abstract string Type { get; }

        public AutomationBase Parent { get; set; }

        public void SetProperty(string name, string value)
        {
            world.SetProperty(name, value, Id);
        }

        public void SetTransition(Dictionary<string, string> transitionProperties, TimeSpan timeSpan)
        {
            world.SetTransition(transitionProperties, timeSpan, Id);
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
