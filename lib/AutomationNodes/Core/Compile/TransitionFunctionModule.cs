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

        public TransitionFunctionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
        }

        private const string TransitionFunctionParameterName = "TransitionFunctionModule.TransitionFunctionParameterName";
        private const string TransitionParameters = "TransitionFunctionModule.TransitionParameters";
        private const string Duration = "TransitionFunctionModule.Duration";
        private readonly TokenParameters transitionFunctiontokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ':', '[', ']', ',' },
            TokenGroups = new List<TokenGroup> { new TokenGroup('(', ')') }
        };

        public void ExpectOpenBraket(Compilation compilation, string token)
        {
            compilation.AddState(TransitionParameters, new Dictionary<string, string>());
            compilation.TokenParameters.Push(transitionFunctiontokenParameters);
            compilation.TokenHandler = ExpectTransitionFunctionParameters;
        }

        private void ExpectTransitionFunctionParameters(Compilation compilation, string token)
        {
            var current = compilation.State;
            if (token.Trim().Length == 0) {
            } else if (token.Is(")")) {
                CompileStatement(compilation);
                compilation.State = new State();
                compilation.State.Variable = current.Variable;
                compilation.TokenParameters.Pop();
                compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
            } else if (token.Is("[") || token.Is(",")) {
                compilation.TokenHandler = ExpectTransitionFunctionParameterPropertyName;
            } else if (token.Is("]")) {
            } else {
                throw new Exception($"Expected transition function parameter but got {token}");
            }
        }

        private void ExpectTransitionFunctionParameterPropertyName(Compilation compilation, string token)
        {
            compilation.AddState(TransitionFunctionParameterName, token.Trim());
            compilation.TokenHandler = ExpectTransitionFunctionParameterPropertySeparator;
        }

        private void ExpectTransitionFunctionParameterPropertySeparator(Compilation compilation, string token)
        {
            if (token.Is(":")) {
                compilation.TokenHandler = ExpectTransitionFunctionParameterPropertyValue;
            } else {
                throw new Exception($"Expected : but got {token} after property name {compilation.GetState(TransitionFunctionParameterName)}");
            }
        }

        private void ExpectTransitionFunctionParameterPropertyValue(Compilation compilation, string token)
        {
            if (compilation.IsState(TransitionFunctionParameterName, "duration")) {
                compilation.AddState(Duration, token);
            } else {
                compilation.GetState<Dictionary<string, string>>(TransitionParameters).Add(compilation.GetState(TransitionFunctionParameterName), token);
            }

            compilation.RemoveState(TransitionFunctionParameterName);
            compilation.TokenHandler = ExpectTransitionFunctionParameters;
        }

        public void CompileInstanceStatement(Compilation compilation, SceneSetTransitionStatement transitionStatement, Dictionary<string, string> parameters)
        {
            var variable = compilation.State.Variable;
            compilation.State = new State();
            compilation.State.Variable = variable;
            if (compilation.State.Variable == null || compilation.State.Variable.Name != transitionStatement.NodeName) {
                compilation.State.Variable = compilation.Variables[transitionStatement.NodeName];
            }
            compilation.AddState(Duration, transitionStatement.Duration.TotalMilliseconds.ToString());
            compilation.AddState(TransitionParameters, transitionStatement.TransitionProperties
                .Select(p => new KeyValuePair<string, string>(p.Key, parameters.TryGetValue(p.Value.Trim(), out var value) ? value : p.Value))
                .ToDictionary());
            CompileStatement(compilation);
        }

        private static void CompileStatement(Compilation compilation)
        {
            var duration = TimeSpan.FromMilliseconds(int.Parse(compilation.GetState(Duration)));
            compilation.StatementsOutput.Peek().Add(new SceneSetTransitionStatement {
                TriggerAt = compilation.SceneTime + compilation.State.Variable.Duration,
                NodeName = compilation.State.Variable.Fullname,
                TransitionProperties = compilation.GetState<Dictionary<string, string>>(TransitionParameters),
                Duration = duration
            });

            if (!compilation.IsCompilingADefinition) {
                compilation.State.Variable.Duration += duration;
            }
        }
    }
}
