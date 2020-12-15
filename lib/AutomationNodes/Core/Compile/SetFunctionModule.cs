using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public interface ISetFunctionModule
    {
        void ExpectOpenBraket(Compilation compilation, string token);
    }

    public class SetFunctionModule : ISetFunctionModule
    {
        private readonly Lazy<IOpeningModule> openingModule;

        public SetFunctionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
        }

        private const string SetFunctionParameterName = "SetFunctionParameterName";
        private const string SetFunctionParameterValue = "SetFunctionParameterValue";
        private readonly TokenParameters setFunctiontokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ':', '[', ']', ',' },
            TokenGroups = new List<TokenGroup> { new TokenGroup('(', ')') }
        };

        public void ExpectOpenBraket(Compilation compilation, string token)
        {
            compilation.TokenParameters.Push(setFunctiontokenParameters);
            compilation.TokenHandler = ExpectSetFunctionParameters;
        }

        private void ExpectSetFunctionParameters(Compilation compilation, string token)
        {
            if (token.Is(")")) {
                compilation.TokenParameters.Pop();
                compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
            } else if (token.Is("[") || token.Is(",")) {
                compilation.TokenHandler = ExpectSetFunctionParameterPropertyName;
            } else if (token.Is("]")) {
            } else {
                throw new Exception($"Expected [ or ) but got {token}");
            }
        }

        private void ExpectSetFunctionParameterPropertyName(Compilation compilation, string token)
        {
            compilation.AddState(SetFunctionParameterName, token);
            compilation.TokenHandler = ExpectSetFunctionParameterPropertySeparator;
        }

        private void ExpectSetFunctionParameterPropertySeparator(Compilation compilation, string token)
        {
            if (token.Is(":")) {
                compilation.TokenHandler = ExpectSetFunctionParameterPropertyValue;
            } else {
                throw new Exception($"Expected : but got {token} after property name {compilation.GetState(SetFunctionParameterName)}");
            }
        }

        private void ExpectSetFunctionParameterPropertyValue(Compilation compilation, string token)
        {
            compilation.AddState(SetFunctionParameterValue, token);
            CompileStatement(compilation);
            compilation.State = new State(compilation.State.Variable);
            compilation.TokenHandler = ExpectSetFunctionParameters;
        }

        private static void CompileStatement(Compilation compilation)
        {
            compilation.StatementsOutput.Peek().Add(new SceneSetPropertyStatement {
                TriggerAt = compilation.SceneTime + compilation.State.Variable.Duration,
                NodeName = compilation.State.Variable.Name,
                PropertyName = compilation.GetState(SetFunctionParameterName),
                PropertyValue = compilation.GetState(SetFunctionParameterValue)
            });
        }
    }
}
