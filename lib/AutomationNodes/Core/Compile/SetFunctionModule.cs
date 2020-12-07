using System;

namespace AutomationNodes.Core.Compile
{
    public interface ISetFunctionModule
    {
        void ExpectSetFunctionParameterPropertyValue(Compilation compilation, string token);
        void ExpectSetFunctionParameterPropertySeparator(Compilation compilation, string token);
        void ExpectSetFunctionParameterPropertyName(Compilation compilation, string token);
        void ExpectSetFunctionParameters(Compilation compilation, string token);
    }

    public class SetFunctionModule : ISetFunctionModule
    {
        private readonly Lazy<ICommonModule> commonModule;

        public SetFunctionModule(IServiceProvider serviceProvider)
        {
            commonModule = new Lazy<ICommonModule>(() => (ICommonModule)serviceProvider.GetService(typeof(ICommonModule)));
        }

        public void ExpectSetFunctionParameterPropertyValue(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;
            current.SetFunctionParameterValue = token;
            commonModule.Value.CompileStatement(compilation);
            compilation.CurrentStatement = new Statement();
            compilation.CurrentStatement.Variable = current.Variable;
            compilation.Expecting = ExpectSetFunctionParameters;
            return;
        }

        public void ExpectSetFunctionParameterPropertySeparator(Compilation compilation, string token)
        {
            if (token != ":") {
                throw new Exception($"Expected : but got {token} after property name {compilation.CurrentStatement.SetFunctionParameterName}");
            }
            compilation.Expecting = ExpectSetFunctionParameterPropertyValue;
            return;
        }

        public void ExpectSetFunctionParameterPropertyName(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;
            if (current.SetFunctionParameterName == null) {
                current.SetFunctionParameterName = token;
                compilation.Expecting = ExpectSetFunctionParameterPropertySeparator;
                return;
            }
            throw new Exception("Expected property name");
        }

        public void ExpectSetFunctionParameters(Compilation compilation, string token)
        {
            if (token == ")") {
                compilation.Expecting = commonModule.Value.ExpectNothingInParticular;
                return;
            } else if (token == "[") {
                compilation.CurrentStatement.ParameterGroup = true;
                compilation.Expecting = ExpectSetFunctionParameterPropertyName;
                return;
            } else if (token == "]") {
                return;
            } else if (token == ",") {
                compilation.Expecting = ExpectSetFunctionParameterPropertyName;
                return;
            }
            throw new Exception($"Expected set function parameter but got {token}");
        }
    }
}
