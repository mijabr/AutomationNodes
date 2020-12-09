using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public interface ITransitionFunctionModule
    {
        void ExpectTransitionFunctionParameterPropertyValue(Compilation compilation, string token);
        void ExpectTransitionFunctionParameterPropertySeparator(Compilation compilation, string token);
        void ExpectTransitionFunctionParameterPropertyName(Compilation compilation, string token);
        void ExpectTransitionFunctionParameters(Compilation compilation, string token);
        void ExpectOpenBraket(Compilation compilation, string token);
    }

    public class TransitionFunctionModule : ITransitionFunctionModule
    {
        private readonly Lazy<IOpeningModule> openingModule;

        public TransitionFunctionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
        }

        private const string TransitionFunctionParameterName = "TransitionFunctionParameterName";
        private const string TransitionParameters = "TransitionParameters";
        private const string Duration = "Duration";

        public void ExpectOpenBraket(Compilation compilation, string token)
        {
            compilation.AddState(TransitionParameters, new Dictionary<string, string>());
            compilation.Expecting = ExpectTransitionFunctionParameters;
        }

        public void ExpectTransitionFunctionParameters(Compilation compilation, string token)
        {
            var current = compilation.State;
            if (token.Is(")")) {
                CompileStatement(compilation);
                compilation.State = new State();
                compilation.State.Variable = current.Variable;
                compilation.Expecting = openingModule.Value.ExpectNothingInParticular;
            } else if (token.Is("[") || token.Is(",")) {
                compilation.Expecting = ExpectTransitionFunctionParameterPropertyName;
            } else if (token.Is("]")) {
            } else {
                throw new Exception($"Expected transition function parameter but got {token}");
            }
        }

        public void ExpectTransitionFunctionParameterPropertyName(Compilation compilation, string token)
        {
            compilation.AddState(TransitionFunctionParameterName, token);
            compilation.Expecting = ExpectTransitionFunctionParameterPropertySeparator;
        }

        public void ExpectTransitionFunctionParameterPropertySeparator(Compilation compilation, string token)
        {
            if (token.Is(":")) {
                compilation.Expecting = ExpectTransitionFunctionParameterPropertyValue;
            } else {
                throw new Exception($"Expected : but got {token} after property name {compilation.GetState(TransitionFunctionParameterName)}");
            }
        }

        public void ExpectTransitionFunctionParameterPropertyValue(Compilation compilation, string token)
        {
            if (compilation.IsState(TransitionFunctionParameterName, "duration")) {
                compilation.AddState(Duration, token);
            } else {
                compilation.GetState<Dictionary<string, string>>(TransitionParameters).Add(compilation.GetState(TransitionFunctionParameterName), token);
            }

            compilation.RemoveState(TransitionFunctionParameterName);
            compilation.Expecting = ExpectTransitionFunctionParameters;
        }

        private static void CompileStatement(Compilation compilation)
        {
            var duration = TimeSpan.FromMilliseconds(int.Parse(compilation.GetState(Duration)));
            compilation.CompiledStatements.Add(new SceneSetTransitionStatement {
                TriggerAt = compilation.SceneTime + compilation.State.Variable.Duration,
                NodeName = compilation.State.Variable.Name,
                TransitionProperties = compilation.GetState<Dictionary<string, string>>(TransitionParameters),
                Duration = duration
            });

            compilation.State.Variable.Duration += duration;
        }
    }
}
