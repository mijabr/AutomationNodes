using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutomationNodes.Core
{
    public class SceneBase
    {
        protected IWorld World { get; private set; }

        public SceneBase(IWorld world)
        {
            this.World = world;
        }

        public void Run(string script)
        {
            var scriptSplit = script.SplitAndTrim(';');
            foreach (var statement in scriptSplit)
            {
                var statementSplit = statement.SplitAndTrim('.');
                var node = CreateNodeFromDeclaration(statementSplit[0]);
                for (var n = 1; n < statementSplit.Length; n++)
                {
                    RunNodeCommand(node, statementSplit[n]);
                }
            }
        }

        private INode CreateNodeFromDeclaration(string declaration)
        {
            var declarationSplit = declaration.SplitAndTrim('(', ')');
            var typeName = declarationSplit[0];
            var parameter = declarationSplit[1];
            var type = Assembly.GetExecutingAssembly().GetTypes().Where(t => string.Equals(t.Name, typeName)).FirstOrDefault();
            var parameters = new object[] { parameter };
            var node = World.CreateNode(type, parameters);
            return node as INode;
        }

        private void RunNodeCommand(INode node, string command)
        {
            var commandSplit = command.SplitAndTrim('(', ')');
            if (string.Equals(commandSplit[0], "set"))
            {
                RunNodeSetCommand(node, commandSplit[1]);
            }
            else if (string.Equals(commandSplit[0], "transition"))
            {
                RunNodeTransitionCommand(node, commandSplit[1]);
            }
        }

        private void RunNodeSetCommand(INode node, string setProperties)
        {
            var setPropertiesSpilt = setProperties.SplitAndTrim('{', '}', ',');
            foreach (var property in setPropertiesSpilt)
            {
                var propertySplit = property.SplitAndTrim(':');
                node.SetProperty(propertySplit[0], propertySplit[1]);
            }
        }

        private void RunNodeTransitionCommand(INode node, string transitionProperties)
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

            node.SetTransition(dictionaryProperties, duration);
        }
    }

    public static class StringExtensions
    {
        public static string[] SplitAndTrim(this string str, params char[] sepatator)
        {
            return str.Split(sepatator).Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
        }
    }
}
