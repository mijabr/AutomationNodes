using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AutomationNodes.Core
{
    public class SceneEvent
    {
        public TimeSpan TriggerAt { get; set; }
        public string NodeName { get; set; }
    }

    public class SceneCreateEvent : SceneEvent
    {
        public Type Type { get; set; }
        public string Parameter { get; set; }
    }

    public class SceneSetPropertyEvent : SceneEvent
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

    public class SceneSetTransitionEvent : SceneEvent
    {
        public Dictionary<string, string> TransitionProperties { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public interface ISceneCompiler
    {
        List<SceneEvent> Compile(string script);
    }

    public class SceneCompiler : ISceneCompiler
    {
        public List<SceneEvent> Compile(string script)
        {
            compilationState = new CompilatonState();

            ScanAssemblyForNodes(Assembly.GetExecutingAssembly());

            var scriptNoComments = string.Join("", script.SplitAndTrim('\r').Where(l => !l.StartsWith(@"//")));
            var scriptSplit = scriptNoComments.SplitAndTrim(';');
            foreach (var statement in scriptSplit)
            {
                var statementSplit = statement.SplitAndTrimEx('.');
                RunStatement(statementSplit);
            }

            return compilationState.Events;
        }

        private class CompilatonState
        {
            public List<SceneEvent> Events { get; set; } = new List<SceneEvent>();

            public TimeSpan SceneTime { get; set; } = TimeSpan.Zero;

            public Dictionary<string, Type> TypesLibrary = new Dictionary<string, Type>();

            public Dictionary<string, NamedNodeInfo> NamedNodes { get; } = new Dictionary<string, NamedNodeInfo>();
        }

        private CompilatonState compilationState;

        private void RunStatement(string[] statementSplit)
        {
            if (statementSplit[0].StartsWith("using "))
            {
                RunUsingCommand(statementSplit[0]);
            }
            else if (statementSplit[0].StartsWith("@"))
            {
                RunAtSymbolCommand(statementSplit[0]);
            }
            else
            {
                RunCommands(statementSplit);
            }
        }

        private void RunUsingCommand(string usingAssembly)
        {
            var usingAssemblySplit = usingAssembly.SplitAndTrim();
            try
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ScanAssemblyForNodes(Assembly.LoadFrom($"{path}\\{usingAssemblySplit[1]}"));
            }
            catch (FileNotFoundException x)
            {
                ScanAssemblyForNodes(Assembly.LoadFrom(usingAssemblySplit[1]));
            }
        }

        private void RunAtSymbolCommand(string atSymbolCommand)
        {
            var commandSplit = atSymbolCommand.GetCommandAndQuotedAndTrim('(', ')');
            compilationState.SceneTime = TimeSpan.FromMilliseconds(int.Parse(commandSplit[1]));
        }

        private void RunCommands(string[] statementSplit)
        {
            var nodeInfo = CreateNodeFromDeclaration(statementSplit[0]);
            for (var n = 1; n < statementSplit.Length; n++)
            {
                RunNodeCommand(nodeInfo, statementSplit[n]);
            }
        }

        private class NamedNodeInfo
        {
            public NamedNodeInfo(string name)
            {
                Name = name;
            }

            public string Name { get; set; }
            private TimeSpan transitionPoint { get; set; } = TimeSpan.Zero;
            public TimeSpan DurationBefore => transitionPoint;
            public void AddDuration(TimeSpan duration)
            {
                if (transitionPoint == null)
                {
                    transitionPoint = duration;
                }
                else
                {
                    transitionPoint += duration;
                }
            }
        }

        private NamedNodeInfo CreateNodeFromDeclaration(string declaration)
        {
            var declarationSplit = declaration.SplitAndTrim('(', ')');
            if (declarationSplit.Length == 1)
            {
                return compilationState.NamedNodes[declarationSplit[0]];
            }
            var typeNameSplit = declarationSplit[0].SplitAndTrim('=');
            string typeName;
            string varName;
            if (typeNameSplit.Length == 1)
            {
                varName = Guid.NewGuid().ToString();
                typeName = declarationSplit[0];
            }
            else
            {
                varName = typeNameSplit[0];
                typeName = typeNameSplit[1];
            }
            compilationState.TypesLibrary.TryGetValue(typeName, out var type);
            if (type == null) throw new Exception($"Unknown node type '{typeName}'. Are you missing a using?");
            var nodeInfo = new NamedNodeInfo(varName);

            compilationState.Events.Add(new SceneCreateEvent
            {
                TriggerAt = compilationState.SceneTime,
                NodeName = varName,
                Type = type,
                Parameter = declarationSplit[1]
            });

            nodeInfo.AddDuration(compilationState.SceneTime);

            compilationState.NamedNodes[varName] = nodeInfo;

            return nodeInfo;
        }

        private void RunNodeCommand(NamedNodeInfo nodeInfo, string command)
        {
            var commandSplit = command.GetCommandAndQuotedAndTrim('(', ')');
            if (string.Equals(commandSplit[0], "set"))
            {
                RunNodeSetCommand(nodeInfo, commandSplit[1]);
            }
            else if (string.Equals(commandSplit[0], "transition"))
            {
                RunNodeTransitionCommand(nodeInfo, commandSplit[1]);
            }
            else if (string.Equals(commandSplit[0], "wait"))
            {
                RunNodeWaitCommand(nodeInfo, commandSplit[1]);
            }
        }

        private void RunNodeSetCommand(NamedNodeInfo nodeInfo, string setProperties)
        {
            var setPropertiesSpilt = setProperties.SplitAndTrim('{', '}', ',');
            foreach (var property in setPropertiesSpilt)
            {
                var propertySplit = property.SplitAndTrim(':');
                compilationState.Events.Add(new SceneSetPropertyEvent
                {
                    TriggerAt = nodeInfo.DurationBefore,
                    NodeName = nodeInfo.Name,
                    PropertyName = propertySplit[0],
                    PropertyValue = propertySplit[1]
                });
            }
        }

        private void RunNodeTransitionCommand(NamedNodeInfo nodeInfo, string transitionProperties)
        {
            var transitionPropertiesSpilt = transitionProperties.SplitAndTrim('{', '}', ',').Where(s => s.Length > 0);
            var dictionaryProperties = new Dictionary<string, string>();
            var duration = TimeSpan.Zero;
            foreach (var property in transitionPropertiesSpilt)
            {
                var propertySplit = property.SplitAndTrim(':');
                if (string.Equals(propertySplit[0], "duration"))
                {
                    duration = TimeSpan.FromMilliseconds(int.Parse(propertySplit[1]));
                }
                else
                {
                    dictionaryProperties[propertySplit[0]] = propertySplit[1];
                }
            }

            compilationState.Events.Add(new SceneSetTransitionEvent
            {
                TriggerAt = nodeInfo.DurationBefore,
                NodeName = nodeInfo.Name,
                TransitionProperties = dictionaryProperties,
                Duration = duration
            });

            nodeInfo.AddDuration(duration);
        }

        private void RunNodeWaitCommand(NamedNodeInfo nodeInfo, string waitProperty)
        {
            var duration = TimeSpan.FromMilliseconds(int.Parse(waitProperty));
            nodeInfo.AddDuration(duration);
        }

        private void ScanAssemblyForNodes(Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in assembly.GetTypes().Where(t => typeof(INode).IsAssignableFrom(t)))
            {
                compilationState.TypesLibrary.Add(type.Name, type);
            }
        }

    }
}
