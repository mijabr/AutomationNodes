using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AutomationNodes.Core
{
    public class SceneBase
    {
        protected IWorld World { get; private set; }

        public SceneBase(IWorld world)
        {
            World = world;
            ScanAssemblyForNodes(Assembly.GetExecutingAssembly());
        }

        public void Run(string script)
        {
            var scriptNoComments = string.Join("", script.SplitAndTrim('\r').Where(l => !l.StartsWith(@"//")));
            var scriptSplit = scriptNoComments.SplitAndTrim(';');
            foreach (var statement in scriptSplit)
            {
                var statementSplit = statement.SplitAndTrimEx('.');
                RunStatement(statementSplit);
            }
        }

        private TimeSpan SceneTime { get; set; }
        private Dictionary<string, Type> typesLibrary = new Dictionary<string, Type>();
        private Dictionary<string, NamedNodeInfo> namedNodes { get; } = new Dictionary<string, NamedNodeInfo>();

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
            SceneTime = TimeSpan.FromMilliseconds(int.Parse(commandSplit[1]));
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
            public NamedNodeInfo()
            {
            }

            public NamedNodeInfo(INode node)
            {
                Node = node;
            }

            public INode Node { get; set; }
            private TimeSpan? transitionPoint { get; set; }
            public TimeSpan? DurationBefore => transitionPoint;
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
                return namedNodes[declarationSplit[0]];
            }
            var typeNameSplit = declarationSplit[0].SplitAndTrim('=');
            string typeName;
            string varName = null;
            if (typeNameSplit.Length == 1)
            {
                typeName = declarationSplit[0];
            }
            else
            {
                varName = typeNameSplit[0];
                typeName = typeNameSplit[1];
            }
            var parameter = declarationSplit[1];
            typesLibrary.TryGetValue(typeName, out var type);
            if (type == null) throw new Exception($"Unknown node type '{typeName}'. Are you missing a using?");
            var parameters = new object[] { parameter };
            var nodeInfo = new NamedNodeInfo();

            Action action = () =>
            {
                if (!(World.CreateNode(type, parameters) is INode node))
                {
                    throw new Exception($"Failed to create node '{typeName}'. Are you passing the correct parameters?");
                }
                nodeInfo.Node = node;
            };

            if (SceneTime > TimeSpan.Zero)
            {
                nodeInfo.AddDuration(SceneTime);
                AddFutureEvent(action, SceneTime);
            }
            else
            {
                action.Invoke();
            }

            if (varName == null)
            {
                varName = Guid.NewGuid().ToString();
            }

            namedNodes[varName] = nodeInfo;

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
                Action action = () => nodeInfo.Node.SetProperty(propertySplit[0], propertySplit[1]);
                if (nodeInfo.DurationBefore.HasValue)
                {
                    AddFutureEvent(action, nodeInfo.DurationBefore.Value);
                }
                else
                {
                    action.Invoke();
                }
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

           Action action = () => nodeInfo.Node.SetTransition(dictionaryProperties, duration);
            if (nodeInfo.DurationBefore.HasValue)
            {
                AddFutureEvent(action, nodeInfo.DurationBefore.Value);
            }
            else
            {
                action.Invoke();
            }

            nodeInfo.AddDuration(duration);
        }

        private void RunNodeWaitCommand(NamedNodeInfo nodeInfo, string waitProperty)
        {
            var duration = TimeSpan.FromMilliseconds(int.Parse(waitProperty));
            nodeInfo.AddDuration(duration);
        }

        private void AddFutureEvent(Action action, TimeSpan when)
        {
            World.AddFutureEvent(new TemporalEvent
            {
                TriggerAt = World.Time + when,
                Action = action
            });
        }

        private void ScanAssemblyForNodes(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var itypes = types.Where(t => t.IsAssignableFrom(typeof(INode)));

            foreach (var type in assembly.GetTypes().Where(t => typeof(INode).IsAssignableFrom(t)))
            {
                typesLibrary.Add(type.Name, type);
            }
        }
    }
}
