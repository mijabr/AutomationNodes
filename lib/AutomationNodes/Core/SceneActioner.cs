using AutomationNodes.Core.Compile;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private class GenericNodeClass
        {
            public string ClassName { get; set; }
            public string[] ConstructorParameters { get; set; }
            public List<CompiledStatement> Statements { get; set; }
        }

        private class RunState
        {
            public string ConnectionId { get; set; }
            private Dictionary<string, INode> NodeVariables { get; } = new();
            public void AddNodeVariable(string nodeName, INode node) => NodeVariables[VariableContext(nodeName)] = node;
            public INode GetNodeVariable(string nodeName) => NodeVariables[VariableContext(nodeName)];
            public Dictionary<string, GenericNodeClass> NodeClasses { get; } = new();
            public CompiledStatement CurrentEvent { get; set; }
            public string CurrentClassVariableName { get; set; }
            private string VariableContext(string variableName) => CurrentClassVariableName != null
                ? $"{CurrentClassVariableName}.variableName"
                : variableName;
        }

        private void Run(IEnumerable<CompiledStatement> events, string connectionId)
        {
            var runState = new RunState { ConnectionId = connectionId };
            foreach(var e in events) {
                runState.CurrentEvent = e;
                RunStatement(runState);
            };
        }

        private void RunStatement(RunState runState)
        {
            if (runState.CurrentEvent.TriggerAt == TimeSpan.Zero) {
                GetStatementAction(runState).Invoke();
            } else {
                AddFutureEvent(GetStatementAction(runState), runState.CurrentEvent.TriggerAt);
            }
        }

        private Action GetStatementAction(RunState runState)
        {
            return runState.CurrentEvent switch
            {
                SceneCreateStatement createStatement => GetCreateAction(runState, createStatement),
                SceneSetPropertyStatement setStatement => GetSetAction(runState, setStatement),
                SceneSetTransitionStatement transitionStatement => GetTransitionAction(runState, transitionStatement),
                _ => throw new NotImplementedException()
            };
        }

        private Action GetCreateAction(RunState runState, SceneCreateStatement sceneCreateStatement)
        {
            return () => {
                var parent = sceneCreateStatement.ParentNodeName != null ? runState.GetNodeVariable(sceneCreateStatement.ParentNodeName) : null;
                if (parent != null) {
                    if (!(nodeCommander.CreateChildNode(sceneCreateStatement.Type, parent, sceneCreateStatement.Parameters) is INode node)) {
                        throw new Exception($"Failed to create child node '{sceneCreateStatement.Type}'");
                    }

                    runState.AddNodeVariable(sceneCreateStatement.NodeName, node);
                } else {
                    if (!(nodeCommander.CreateNode(sceneCreateStatement.Type, runState.ConnectionId, sceneCreateStatement.Parameters) is INode node)) {
                        throw new Exception($"Failed to create node '{sceneCreateStatement.Type}'");
                    }

                    runState.AddNodeVariable(sceneCreateStatement.NodeName, node);
                }
            };
        }

        private Action GetSetAction(RunState runState, SceneSetPropertyStatement setStatement)
        {
            return () => {
                var node = runState.GetNodeVariable(setStatement.NodeName);
                nodeCommander.SetProperty(node, setStatement.PropertyName, setStatement.PropertyValue);
            };
        }

        private Action GetTransitionAction(RunState runState, SceneSetTransitionStatement transitionStatement)
        {
            return () => {
                var node = runState.GetNodeVariable(transitionStatement.NodeName);
                nodeCommander.SetTransition(node, transitionStatement.TransitionProperties, transitionStatement.Duration);
            };
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
