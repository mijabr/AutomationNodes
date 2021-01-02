using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public interface ITransitionFunctionModule
    {
        void ExpectOpenBraket(Compilation compilation, string token);
        void CompileInstanceStatement(Compilation compilation, SceneSetTransitionStatement transitionStatement, Dictionary<string, string> functionParameters);
    }

    public class TransitionFunctionModule : ITransitionFunctionModule
    {
        private readonly Lazy<IOpeningModule> openingModule;
        private readonly Lazy<IParameterModule> parameterModule;

        public TransitionFunctionModule(IServiceProvider serviceProvider)
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
            var duration = propertyParameters["duration"].ToTimeSpan();
            propertyParameters.Remove("duration");
            CompileStatement(compilation, propertyParameters, duration);
            compilation.State = new State(compilation.State.Variable);
            compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
        }

        public void CompileInstanceStatement(Compilation compilation, SceneSetTransitionStatement transitionStatement, Dictionary<string, string> parameters)
        {
            var variable = compilation.State.Variable;
            compilation.State = new State();
            compilation.State.Variable = variable;
            if (compilation.State.Variable == null || compilation.State.Variable.Name != transitionStatement.NodeName) {
                compilation.State.Variable = compilation.Variables[transitionStatement.NodeName];
            }
            var transitionParameters = transitionStatement.TransitionProperties
                .Select(p => new KeyValuePair<string, string>(p.Key, parameters.TryGetValue(p.Value.Trim(), out var value) ? value : p.Value))
                .ToDictionary();
            CompileStatement(compilation, transitionParameters, transitionStatement.Duration);
        }

        private static void CompileStatement(Compilation compilation, Dictionary<string, string> transitionParameters, TimeSpan duration)
        {
            compilation.StatementsOutput.Peek().Add(new SceneSetTransitionStatement {
                TriggerAt = compilation.SceneTime + compilation.State.Variable.Duration,
                NodeName = compilation.State.Variable.Fullname,
                TransitionProperties = transitionParameters,
                Duration = duration
            });

            if (!compilation.IsCompilingADefinition) {
                compilation.State.Variable.Duration += duration;
            }
        }
    }
}
