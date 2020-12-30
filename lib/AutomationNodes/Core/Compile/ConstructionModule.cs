using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public interface IConstructionModule
    {
        void ExpectVarName(Compilation compilation, string token);
        void StartParameterParse(Compilation compilation, Type type);
        void CompileInstanceStatement(Compilation compilation, SceneCreateStatement createStatement, Dictionary<string, string> parameters, string scope, string parentNodeName = null);
    }

    public class ConstructionModule : IConstructionModule
    {
        private readonly Lazy<IOpeningModule> openingModule;
        private readonly Lazy<IParameterModule> parameterModule;

        public ConstructionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
            parameterModule = new Lazy<IParameterModule>(() => (IParameterModule)serviceProvider.GetService(typeof(IParameterModule)));
        }

        private const string TheType = "ConstructionModule.TheType";
        private const string ParentNodename = "ConstructionModule.ParentNodename";

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
                compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
            } else {
                throw new Exception($"Expected = but got {token}");
            }
        }

        public void StartParameterParse(Compilation compilation, Type type)
        {
            compilation.AddState(TheType, type);
            compilation.AddState<string>(ParentNodename, null);
            parameterModule.Value.StartParameterParse(compilation, OnCompleteParameterParse);
        }

        private void OnCompleteParameterParse(Compilation compilation, List<string> parameters)
        {
            CompileStatement(compilation, compilation.GetState<Type>(TheType), parameters);
            compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
        }

        public void CompileInstanceStatement(Compilation compilation, SceneCreateStatement createStatement, Dictionary<string, string> parameters, string scope, string parentNodeName = null)
        {
            compilation.State = new State();
            compilation.NewVariable(createStatement.NodeName, scope);
            compilation.AddState(ParentNodename, parentNodeName);
            CompileStatement(compilation, createStatement.Type, createStatement.Parameters.Select(p => parameters.TryGetValue(p, out var value) ? value : p).ToList());
        }

        private static void CompileStatement(Compilation compilation, Type type, List<string> parameterValues)
        {
            if (compilation.State.Variable == null) {
                compilation.NewVariable(Guid.NewGuid().ToString());
            }

            var variableName = compilation.State.Variable.Fullname;

            compilation.StatementsOutput.Peek().Add(new SceneCreateStatement {
                TriggerAt = compilation.SceneTime,
                NodeName = variableName,
                Type = type,
                Parameters = parameterValues.ToArray(),
                ParentNodeName = compilation.GetState(ParentNodename)
            });
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
