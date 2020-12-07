using AutomationNodes.Core.Compile;
using System;
using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public interface ISceneActioner
    {
        void Run(string script, string connectionId);
    }

    public class SceneActioner : ISceneActioner
    {
        private readonly IWorldTime worldTime;
        private readonly ITemporalEventQueue temporalEventQueue;
        private readonly ISceneCompiler sceneCompiler;
        private readonly INodeCommander nodeCommander;

        public SceneActioner(
            IWorldTime worldTime,
            ITemporalEventQueue temporalEventQueue,
            ISceneCompiler sceneCompiler,
            INodeCommander nodeCommander)
        {
            this.worldTime = worldTime;
            this.temporalEventQueue = temporalEventQueue;
            this.sceneCompiler = sceneCompiler;
            this.nodeCommander = nodeCommander;
        }

        public void Run(string script, string connectionId)
        {
            Run(sceneCompiler.Compile(script), connectionId);
        }

        private class RunState
        {
            public string ConnectionId { get; set; }
            public Dictionary<string, INode> NodeVariables { get; } = new Dictionary<string, INode>();
            public SceneStatement CurrentEvent { get; set; }
        }

        private void Run(List<SceneStatement> events, string connectionId)
        {
            var runState = new RunState { ConnectionId = connectionId };
            events.ForEach(e => {
                runState.CurrentEvent = e;
                FireEvent(runState);
            });
        }

        private void FireEvent(RunState runState)
        {
            if (runState.CurrentEvent.TriggerAt == TimeSpan.Zero)
            {
                BuildEventAction(runState).Invoke();
            }
            else
            {
                AddFutureEvent(BuildEventAction(runState), runState.CurrentEvent.TriggerAt);
            }
        }

        private Action BuildEventAction(RunState runState)
        {
            Action action = runState.CurrentEvent switch
            {
                SceneCreateStatement createEvent => () => CreateEventAction(runState, createEvent),
                SceneSetPropertyEvent setEvent => () => SetEventAction(runState, setEvent),
                SceneSetTransitionEvent transitionEvent => () => TransitionAction(runState, transitionEvent),
                _ => throw new NotImplementedException()
            };

            return action;
        }

        private void CreateEventAction(RunState runState, SceneCreateStatement sceneCreateEvent)
        {
            if (!(nodeCommander.CreateNode(sceneCreateEvent.Type, runState.ConnectionId, sceneCreateEvent.Parameters) is INode node))
            {
                throw new Exception($"Failed to create node '{sceneCreateEvent.Type}'");
            }

            runState.NodeVariables[sceneCreateEvent.NodeName] = node;
        }

        private void SetEventAction(RunState runState, SceneSetPropertyEvent setEvent)
        {
            var node = runState.NodeVariables[setEvent.NodeName];
            nodeCommander.SetProperty(node, setEvent.PropertyName, setEvent.PropertyValue);
        }

        private void TransitionAction(RunState runState, SceneSetTransitionEvent transitionEvent)
        {
            var node = runState.NodeVariables[transitionEvent.NodeName];
            nodeCommander.SetTransition(node, transitionEvent.TransitionProperties, transitionEvent.Duration);
        }

        private void AddFutureEvent(Action action, TimeSpan when)
        {
            temporalEventQueue.AddFutureEvent(new TemporalEvent
            {
                TriggerAt = worldTime.Time.Elapsed + when,
                Action = action
            });
        }
    }
}
