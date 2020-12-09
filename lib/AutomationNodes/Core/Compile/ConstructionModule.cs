using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public interface IConstructionModule
    {
        void ExpectVarName(Compilation compilation, string token);
        void ExpectTypeName(Compilation compilation, string token);
        void ExpectOpenBracket(Compilation compilation, string token);
        void ExpectConstructorParameters(Compilation compilation, string token);
    }

    public class ConstructionModule : IConstructionModule
    {
        private readonly Lazy<IOpeningModule> commonModule;

        public ConstructionModule(IServiceProvider serviceProvider)
        {
            commonModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
        }

        private const string TypeName = "TypeName";
        private const string ConstructorParameters = "ConstructorParameters";

        public void ExpectVarName(Compilation compilation, string token)
        {
            var current = compilation.State;
            if (current.Variable == null) {
                current.Variable = new Variable { Name = token };
                compilation.Variables.Add(current.Variable.Name, current.Variable);
                compilation.Expecting = ExpectAssignment;
            } else {
                throw new Exception("variable already named");
            }
        }

        private void ExpectAssignment(Compilation compilation, string token)
        {
            if (token == "=") {
                compilation.Expecting = commonModule.Value.ExpectNothingInParticular;
            } else {
                throw new Exception($"Expected = but got {token}");
            }
        }

        public void ExpectTypeName(Compilation compilation, string token)
        {
            compilation.AddState(TypeName, token);
            compilation.Expecting = ExpectOpenBracket;
        }

        public void ExpectOpenBracket(Compilation compilation, string token)
        {
            if (!token.Is("(")) {
                throw new Exception($"Expected ( but got {token}");
            }

            compilation.AddState(ConstructorParameters, new List<string>());
            compilation.Expecting = ExpectConstructorParameters;
        }

        public void ExpectConstructorParameters(Compilation compilation, string token)
        {
            if (token == ")") {
                CompileStatement(compilation);
                compilation.Expecting = commonModule.Value.ExpectNothingInParticular;
            } else if (token != ",") {
                compilation.GetState<List<string>>(ConstructorParameters).Add(token);
            }
        }

        private static void CompileStatement(Compilation compilation)
        {
            if (!compilation.TypesLibrary.TryGetValue(compilation.GetState(TypeName), out var type)) {
                throw new Exception($"Unknown node type '{compilation.GetState(TypeName)}'. Are you missing a using?");
            }

            compilation.CompiledStatements.Add(new SceneCreateStatement {
                TriggerAt = compilation.SceneTime,
                NodeName = compilation.State.Variable.Name,
                Type = type,
                Parameters = compilation.GetState<List<string>>(ConstructorParameters).ToArray()
            });
        }
    }
}
