using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public interface ISetFunctionModule
    {
        void ExpectOpenBraket(Compilation compilation, string token);
        void CompileInstanceStatement(Compilation compilation, SceneSetPropertyStatement setStatement, Dictionary<string, string> functionParameters);
    }

    public class SetFunctionModule : ISetFunctionModule
    {
        private readonly Lazy<IOpeningModule> openingModule;
        private readonly Lazy<IParameterModule> parameterModule;

        public SetFunctionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
            parameterModule = new Lazy<IParameterModule>(() => (IParameterModule)serviceProvider.GetService(typeof(IParameterModule)));
        }

        public void ExpectOpenBraket(Compilation compilation, string token)
        {
            if (!token.Is("(")) {
                throw new Exception($"Expected ( but got {token}");
            }

            parameterModule.Value.StartPropertyParameterParse(compilation, OnParameterParseComplete);
        }

        private void OnParameterParseComplete(Compilation compilation, Dictionary<string, string> propertyParameters)
        {
            foreach (var propertySetter in propertyParameters) {
                CompileStatement(compilation, propertySetter.Key, propertySetter.Value);
                compilation.State = new State(compilation.State.Variable);
            }

            compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
        }

        public void CompileInstanceStatement(Compilation compilation, SceneSetPropertyStatement setStatement, Dictionary<string, string> parameters)
        {
            var variable = compilation.State.Variable;
            compilation.State = new State();
            compilation.State.Variable = variable;
            if (compilation.State.Variable == null || compilation.State.Variable.Name != setStatement.NodeName) {
                compilation.State.Variable = compilation.Variables[setStatement.NodeName];
            }

            CompileStatement(compilation, setStatement.PropertyName, setStatement.PropertyValue.ReplaceTokenUsingParameters(parameters));
        }

        private static void CompileStatement(Compilation compilation, string name, string value)
        {
            compilation.StatementsOutput.Peek().Add(new SceneSetPropertyStatement {
                TriggerAt = compilation.SceneTime + compilation.State.Variable.Duration,
                NodeName = compilation.State.Variable.Fullname,
                PropertyName = name,
                PropertyValue = value
            });
        }
    }

    public static class StringExtensions
    {
        public static string ReplaceTokenUsingParameters(this string token, Dictionary<string, string> parameters) =>
            parameters.TryGetValue(token.Trim(), out var value) ? value : token;
    }
}
