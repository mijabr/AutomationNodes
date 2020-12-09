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
        private readonly Lazy<IOpeningModule> openingModule;

        public SetFunctionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
        }

        private const string SetFunctionParameterName = "SetFunctionParameterName";
        private const string SetFunctionParameterValue = "SetFunctionParameterValue";

        public void ExpectSetFunctionParameterPropertyValue(Compilation compilation, string token)
        {
            compilation.AddState(SetFunctionParameterValue, token);
            CompileStatement(compilation);
            compilation.State = new State(compilation.State.Variable);
            compilation.Expecting = ExpectSetFunctionParameters;
        }

        public void ExpectSetFunctionParameterPropertySeparator(Compilation compilation, string token)
        {
            if (token.Is(":")) {
                compilation.Expecting = ExpectSetFunctionParameterPropertyValue;
            } else {
                throw new Exception($"Expected : but got {token} after property name {compilation.GetState(SetFunctionParameterName)}");
            }
        }

        public void ExpectSetFunctionParameterPropertyName(Compilation compilation, string token)
        {
            compilation.AddState(SetFunctionParameterName, token);
            compilation.Expecting = ExpectSetFunctionParameterPropertySeparator;
        }

        public void ExpectSetFunctionParameters(Compilation compilation, string token)
        {
            if (token.Is(")")) {
                compilation.Expecting = openingModule.Value.ExpectNothingInParticular;
            } else if (token.Is("[") || token.Is(",")) {
                compilation.Expecting = ExpectSetFunctionParameterPropertyName;
            } else if (token.Is("]")) {
            } else {
                throw new Exception($"Expected set function parameter but got {token}");
            }
        }

        private static void CompileStatement(Compilation compilation)
        {
            compilation.CompiledStatements.Add(new SceneSetPropertyStatement {
                TriggerAt = compilation.SceneTime + compilation.State.Variable.Duration,
                NodeName = compilation.State.Variable.Name,
                PropertyName = compilation.GetState(SetFunctionParameterName),
                PropertyValue = compilation.GetState(SetFunctionParameterValue)
            });
        }
    }
}
