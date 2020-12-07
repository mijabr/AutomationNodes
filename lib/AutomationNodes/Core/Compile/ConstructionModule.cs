using System;

namespace AutomationNodes.Core.Compile
{
    public interface IConstructionModule
    {
        void ExpectVar(Compilation compilation, string token);
        void ExpectAssignment(Compilation compilation, string token);
        void ExpectConstructorOpenBracketOrDotOrAt(Compilation compilation, string token);
        void ExpectConstructorParameters(Compilation compilation, string token);
    }

    public class ConstructionModule : IConstructionModule
    {
        private readonly Lazy<ICommonModule> commonModule;

        public ConstructionModule(IServiceProvider serviceProvider)
        {
            commonModule = new Lazy<ICommonModule>(() => (ICommonModule)serviceProvider.GetService(typeof(ICommonModule)));
        }

        public void ExpectVar(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;
            if (current.Variable == null) {
                current.Variable = new Variable { Name = token };
                compilation.Variables.Add(current.Variable.Name, current.Variable);
                compilation.Expecting = ExpectAssignment;
                return;
            }
            throw new Exception("variable already named");
        }

        public void ExpectAssignment(Compilation compilation, string token)
        {
            if (token == "=") {
                compilation.Expecting = commonModule.Value.ExpectNothingInParticular;
                return;
            }
            throw new Exception($"Expected = but got {token}");
        }

        public void ExpectConstructorOpenBracketOrDotOrAt(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;

            if (token == "(") {
                if (current.Token == "@") {
                    compilation.Expecting = commonModule.Value.ExpectAtParameter;
                    return;
                }
                current.TypeName = current.Token;
                if (current.Variable == null) {
                    current.Variable = new Variable { Name = Guid.NewGuid().ToString() };
                    compilation.Variables.Add(current.Variable.Name, current.Variable);
                }
                compilation.Expecting = ExpectConstructorParameters;
                return;
            } else if (token == ".") {
                current.Variable = compilation.Variables[current.Token];
                compilation.Expecting = commonModule.Value.ExpectFunctionName;
                return;
            }
            throw new Exception($"Expected . or ( after {current.Token}");
        }

        public void ExpectConstructorParameters(Compilation compilation, string token)
        {
            if (token == ")") {
                compilation.Expecting = commonModule.Value.ExpectNothingInParticular;
            } else if (token != ",") {
                compilation.CurrentStatement.Parameter.Add(token);
            }
            return;
        }
    }
}
