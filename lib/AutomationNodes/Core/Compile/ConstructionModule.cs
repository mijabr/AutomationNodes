using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public interface IConstructionModule
    {
        void ExpectVarName(Compilation compilation, string token);
        void ExpectTypeName(Compilation compilation, string token);
        void ExpectOpenBracket(Compilation compilation, string token);
    }

    public class ConstructionModule : IConstructionModule
    {
        private readonly Lazy<IOpeningModule> commonModule;

        public ConstructionModule(IServiceProvider serviceProvider)
        {
            commonModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
        }

        private const string TypeName = "TypeName";
        private const string ConstructorParameters = "ConstructorParameters";
        private readonly TokenParameters constructorTokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ',' }
        };

        public void ExpectVarName(Compilation compilation, string token)
        {
            var current = compilation.State;
            if (current.Variable == null) {
                current.Variable = new Variable { Name = token };
                compilation.Variables.Add(current.Variable.Name, current.Variable);
                compilation.TokenHandler = ExpectAssignment;
            } else {
                throw new Exception("variable already named");
            }
        }

        private void ExpectAssignment(Compilation compilation, string token)
        {
            if (token == "=") {
                compilation.TokenHandler = commonModule.Value.ExpectNothingInParticular;
            } else {
                throw new Exception($"Expected = but got {token}");
            }
        }

        public void ExpectTypeName(Compilation compilation, string token)
        {
            compilation.AddState(TypeName, token);
            compilation.TokenHandler = ExpectOpenBracket;
        }

        public void ExpectOpenBracket(Compilation compilation, string token)
        {
            if (!token.Is("(")) {
                throw new Exception($"Expected ( but got {token}");
            }

            compilation.AddState(ConstructorParameters, new List<string>());
            compilation.TokenParameters.Push(constructorTokenParameters);
            compilation.TokenHandler = ExpectConstructorParameters;
        }

        private void ExpectConstructorParameters(Compilation compilation, string token)
        {
            if (token == ")") {
                CompileStatement(compilation);
                compilation.TokenParameters.Pop();
                compilation.TokenHandler = commonModule.Value.ExpectNothingInParticular;
            } else if (token != ",") {
                compilation.GetState<List<string>>(ConstructorParameters).Add(token.Trim());
            }
        }

        private static void CompileStatement(Compilation compilation)
        {
            var constructorParameters = compilation.GetState<List<string>>(ConstructorParameters);

            compilation.Functions.TryGetValue(compilation.GetState(TypeName), out var function);
            if (function != null) {
                foreach (var statement in function.Statements) {
                    var parameterValues = function.ConstructorParameters.Select((p, index) =>
                        new KeyValuePair<string, string>($"%{p}%", index < constructorParameters.Count ? constructorParameters[index] : string.Empty)).ToDictionary();
                    compilation.StatementsOutput.Peek().Add(statement.GenerateInstanceStatement(null, parameterValues));
                }
                return;
            }

            Type type = null;
            compilation.Classes.TryGetValue(compilation.GetState(TypeName), out var nodeClass);
            if (nodeClass != null) {
                type = typeof(GenericNode);
            } else {
                type = compilation.TypesLibrary.TryGetValue(compilation.GetState(TypeName), out var t) ? t : null;
            }

            if (type == null) {
                throw new Exception($"Unknown node type '{compilation.GetState(TypeName)}'. Are you missing a using?");
            }

            var variableName = compilation.State.Variable.Name;

            compilation.StatementsOutput.Peek().Add(new SceneCreateStatement {
                TriggerAt = compilation.SceneTime,
                NodeName = variableName,
                Class = nodeClass?.ClassName,
                Type = type,
                Parameters = constructorParameters.ToArray()
            });

            if (nodeClass != null) {
                foreach (var statement in nodeClass.Statements) {
                    var parameterValues = nodeClass.ConstructorParameters.Select((p, index) =>
                        new KeyValuePair<string, string>($"%{p}%", constructorParameters[index])).ToDictionary();
                    compilation.StatementsOutput.Peek().Add(statement.GenerateInstanceStatement(variableName, parameterValues));
                }
            }
        }
    }

    public static class KeyValuePairExtensions
    {
        public static Dictionary<TK, TV> ToDictionary<TK, TV>(this IEnumerable<KeyValuePair<TK, TV>> keyValuePairs)
        {
            return keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }

}
