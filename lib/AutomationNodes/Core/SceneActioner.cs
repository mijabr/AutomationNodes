using AutomationNodes.Core.Compile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core
{
    public interface ISceneActioner
    {
        void Run(Clients clients, string script);
    }

    public class SceneActioner : ISceneActioner
    {
        private readonly IWorldTime worldTime;
        private readonly ITemporalEventQueue temporalEventQueue;
        private readonly ISceneCompiler sceneCompiler;
        private readonly INodeOrchestrator nodeOrchestrator;

        public SceneActioner(
            IWorldTime worldTime,
            ITemporalEventQueue temporalEventQueue,
            ISceneCompiler sceneCompiler,
            INodeOrchestrator nodeOrchestrator)
        {
            this.worldTime = worldTime;
            this.temporalEventQueue = temporalEventQueue;
            this.sceneCompiler = sceneCompiler;
            this.nodeOrchestrator = nodeOrchestrator;
        }

        public void Run(Clients clients, string script)
        {
            Run(clients, sceneCompiler.Compile(script));
        }

        private class GenericNodeClass
        {
            public string ClassName { get; set; }
            public string[] ConstructorParameters { get; set; }
            public List<CompiledStatement> Statements { get; set; }
        }

        private class RunState
        {
            public RunState(Clients clients)
            {
                Clients = clients;
            }

            public Clients Clients { get; }
            private Dictionary<string, IClientNode> NodeVariables { get; } = new();
            public void AddNodeVariable(string nodeName, IClientNode node) => NodeVariables[VariableContext(nodeName)] = node;
            public IClientNode GetNodeVariable(string nodeName) => NodeVariables[VariableContext(nodeName)];
            public Dictionary<string, GenericNodeClass> NodeClasses { get; } = new();
            public CompiledStatement CurrentStatement { get; set; }
            public string CurrentClassVariableName { get; set; }
            private string VariableContext(string variableName) => CurrentClassVariableName != null
                ? $"{CurrentClassVariableName}.variableName"
                : variableName;
        }

        private void Run(Clients clients, IEnumerable<CompiledStatement> compiledStatements)
        {
            var runState = new RunState(clients);
            foreach(var statement in compiledStatements) {
                runState.CurrentStatement = statement;
                RunStatement(runState);
            };
        }

        private void RunStatement(RunState runState)
        {
            if (runState.CurrentStatement.TriggerAt == TimeSpan.Zero) {
                GetStatementAction(runState).Invoke();
            } else {
                AddFutureEvent(GetStatementAction(runState), runState.CurrentStatement.TriggerAt);
            }
        }

        private Action GetStatementAction(RunState runState)
        {
            return runState.CurrentStatement switch
            {
                SceneCreateStatement createStatement => GetCreateAction(runState, createStatement),
                SceneSetPropertyStatement setStatement => GetSetAction(runState, setStatement),
                SceneSetTransitionStatement transitionStatement => GetTransitionAction(runState, transitionStatement),
                SceneKeyframeStatement keyframeStatement => GetKeyframeAction(runState, keyframeStatement),
                _ => throw new NotImplementedException()
            };
        }

        private Action GetCreateAction(RunState runState, SceneCreateStatement sceneCreateStatement)
        {
            return () => {
                var parent = sceneCreateStatement.ParentNodeName != null ? runState.GetNodeVariable(sceneCreateStatement.ParentNodeName) : null;
                if (parent != null) {
                    if (!(nodeOrchestrator.CreateChildNode(sceneCreateStatement.Type, runState.Clients, parent, sceneCreateStatement.Parameters) is IClientNode node)) {
                        throw new Exception($"Failed to create child node '{sceneCreateStatement.Type}'");
                    }

                    runState.AddNodeVariable(sceneCreateStatement.NodeName, node);
                } else {
                    if (!(nodeOrchestrator.CreateNode(sceneCreateStatement.Type, runState.Clients, sceneCreateStatement.Parameters) is IClientNode node)) {
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
                nodeOrchestrator.SetProperty(runState.Clients, node, setStatement.PropertyName, setStatement.PropertyValue);
            };
        }

        private Action GetTransitionAction(RunState runState, SceneSetTransitionStatement transitionStatement)
        {
            return () => {
                var node = runState.GetNodeVariable(transitionStatement.NodeName);
                nodeOrchestrator.SetTransition(runState.Clients, node, transitionStatement.TransitionProperties, transitionStatement.Duration);
            };
        }

        private Action GetKeyframeAction(RunState runState, SceneKeyframeStatement keyframeStatement)
        {
            return () => {
                nodeOrchestrator.AddKeyframe(keyframeStatement.KeyframeProperties, keyframeStatement.KeyframeName, keyframeStatement.KeyframePercent);
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
