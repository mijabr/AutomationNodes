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
    }

    public class TransitionFunctionModule : ITransitionFunctionModule
    {
        private readonly Lazy<ICommonModule> commonModule;

        public TransitionFunctionModule(IServiceProvider serviceProvider)
        {
            commonModule = new Lazy<ICommonModule>(() => (ICommonModule)serviceProvider.GetService(typeof(ICommonModule)));
        }

        public void ExpectTransitionFunctionParameterPropertyValue(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;
            current.TransitionFunctionParameterValue = token;
            if (string.Equals(current.TransitionFunctionParameterName, "duration", StringComparison.InvariantCultureIgnoreCase)) {
                current.Duration = current.TransitionFunctionParameterValue;
            } else {
                if (current.TransitionParameters == null) {
                    current.TransitionParameters = new Dictionary<string, string>();
                }
                current.TransitionParameters.Add(current.TransitionFunctionParameterName, current.TransitionFunctionParameterValue);
            }
            current.TransitionFunctionParameterName = null;
            current.TransitionFunctionParameterValue = null;
            compilation.Expecting = ExpectTransitionFunctionParameters;
            return;
        }

        public void ExpectTransitionFunctionParameterPropertySeparator(Compilation compilation, string token)
        {
            if (token != ":") {
                throw new Exception($"Expected : but got {token} after property name {compilation.CurrentStatement.TransitionFunctionParameterName}");
            }
            compilation.Expecting = ExpectTransitionFunctionParameterPropertyValue;
            return;
        }

        public void ExpectTransitionFunctionParameterPropertyName(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;
            if (current.TransitionFunctionParameterName == null) {
                current.TransitionFunctionParameterName = token;
                compilation.Expecting = ExpectTransitionFunctionParameterPropertySeparator;
                return;
            }
            throw new Exception($"Expected transition property name but got {token}");
        }

        public void ExpectTransitionFunctionParameters(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;
            if (token == ")") {
                commonModule.Value.CompileStatement(compilation);
                compilation.CurrentStatement = new Statement();
                compilation.CurrentStatement.Variable = current.Variable;
                compilation.Expecting = commonModule.Value.ExpectNothingInParticular;
                return;
            } else if (token == "[") {
                current.ParameterGroup = true;
                compilation.Expecting = ExpectTransitionFunctionParameterPropertyName;
                return;
            } else if (token == "]") {
                return;
            } else if (token == ",") {
                compilation.Expecting = ExpectTransitionFunctionParameterPropertyName;
                return;
            }
            throw new Exception($"Expected transition function parameter but got {token}");
        }
    }
}
