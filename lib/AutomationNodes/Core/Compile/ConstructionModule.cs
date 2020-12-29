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
        int CompileInstanceStatement(Compilation compilation, SceneCreateStatement createStatement, Dictionary<string, string> parameters, string scope, string parentNodeName = null);
    }

    public class ConstructionModule : IConstructionModule
    {
        private readonly Lazy<IOpeningModule> commonModule;
        private readonly Lazy<IFunctionModule> functionModule;
        private readonly Lazy<IClassModule> classModule;

        public ConstructionModule(IServiceProvider serviceProvider)
        {
            commonModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
            functionModule = new Lazy<IFunctionModule>(() => (IFunctionModule)serviceProvider.GetService(typeof(IFunctionModule)));
            classModule = new Lazy<IClassModule>(() => (IClassModule)serviceProvider.GetService(typeof(IClassModule)));
        }

        private const string TypeName = "TypeName";
        private const string ConstructorParameters = "ConstructorParameters";
        private const string ParentNodename = "ParentNodename";
        private readonly TokenParameters constructorTokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ',' }
        };

        public void ExpectVarName(Compilation compilation, string token)
        {
            if (compilation.State.Variable == null) {
                compilation.NewVariable(token);
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
                compilation.AddState<string>(ParentNodename, null);
                CompileStatement(compilation);
                compilation.TokenParameters.Pop();
                compilation.TokenHandler = commonModule.Value.ExpectNothingInParticular;
            } else if (token != ",") {
                compilation.GetState<List<string>>(ConstructorParameters).Add(token.Trim());
            }
        }

        public int CompileInstanceStatement(Compilation compilation, SceneCreateStatement createStatement, Dictionary<string, string> parameters, string scope, string parentNodeName = null)
        {
            compilation.State = new State();
            compilation.NewVariable(createStatement.NodeName, scope);
            compilation.AddState(TypeName, createStatement.Type.Name.ToString());
            compilation.AddState(ConstructorParameters, createStatement.Parameters.Select(p => parameters.TryGetValue(p, out var value) ? value : p).ToList());
            compilation.AddState(ParentNodename, parentNodeName);
            CompileStatement(compilation);
            return 0;
        }

        private void CompileStatement(Compilation compilation)
        {
            var constructorParameters = compilation.GetState<List<string>>(ConstructorParameters);
            compilation.Functions.TryGetValue(compilation.GetState(TypeName), out var function);
            if (function != null) {
                functionModule.Value.CompileFunctionInstance(compilation, function, constructorParameters);
                return;
            }

            if (compilation.State.Variable == null) {
                compilation.NewVariable(Guid.NewGuid().ToString());
            }

            Type type;
            compilation.Classes.TryGetValue(compilation.GetState(TypeName), out var nodeClass);
            if (nodeClass != null) {
                type = typeof(GenericNode);
            } else {
                type = compilation.TypesLibrary.TryGetValue(compilation.GetState(TypeName), out var t) ? t : null;
            }

            if (type == null) {
                throw new Exception($"Unknown node type '{compilation.GetState(TypeName)}'. Are you missing a using?");
            }

            var variableName = compilation.State.Variable.Fullname;

            compilation.StatementsOutput.Peek().Add(new SceneCreateStatement {
                TriggerAt = compilation.SceneTime,
                NodeName = variableName,
                Class = nodeClass?.ClassName,
                Type = type,
                Parameters = constructorParameters.ToArray(),
                ParentNodeName = compilation.GetState(ParentNodename)
            });

            if (nodeClass != null) {
                classModule.Value.CompileClassInstance(compilation, nodeClass, constructorParameters);
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
