using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public interface IFunctionModule
    {
        void ExpectFunctionName(Compilation compilation, string token);
    }

    public class FunctionModule : IFunctionModule
    {
        private readonly Lazy<IOpeningModule> openingModule;

        public FunctionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
        }

        private const string FunctionName = "FunctionName";
        private const string FunctionConstructorParameters = "FunctionConstructorParameters";
        private const string FunctionStatements = "FunctionStatements";
        private readonly TokenParameters constructorTokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ',' }
        };

        public void ExpectFunctionName(Compilation compilation, string token)
        {
            compilation.AddState(FunctionName, token);
            compilation.TokenHandler = ExpectOpenBracket;
        }

        private void ExpectOpenBracket(Compilation compilation, string token)
        {
            if (token.Is("(")) {
                compilation.AddState<List<string>>(FunctionConstructorParameters, new List<string>());
                compilation.TokenParameters.Push(constructorTokenParameters);
                compilation.TokenHandler = ExpectingConstructorParameters;
            } else {
                throw new Exception($"Expected ( but got {token}");
            }
        }

        private void ExpectingConstructorParameters(Compilation compilation, string token)
        {
            if (token.Is(")")) {
                compilation.TokenParameters.Pop();
                compilation.TokenHandler = ExpectOpenBrace;
            } else if (!token.Is(",")) {
                compilation.GetState<List<string>>(FunctionConstructorParameters).Add(token.Trim());
            }
        }

        private void ExpectOpenBrace(Compilation compilation, string token)
        {
            if (!token.Is("{")) {
                throw new Exception($"Expected ( but got {token}");
            }

            compilation.States.Push(new State());
            compilation.StatementsOutput.Push(new List<CompiledStatement>());
            compilation.TokenHandler = ExpectFunctionDefinition;
            compilation.TokenHandlers.Push(openingModule.Value.ExpectNothingInParticular);
        }

        private void ExpectFunctionDefinition(Compilation compilation, string token)
        {
            if (token.Is("}")) {
                compilation.States.Pop();
                compilation.AddState(FunctionStatements, compilation.StatementsOutput.Pop());
                CompileStatement(compilation);
                compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
            }
        }

        private void CompileStatement(Compilation compilation)
        {
            var functionName = compilation.GetState(FunctionName);
            compilation.Functions.Add(functionName, new Function {
                FunctionName = functionName,
                ConstructorParameters = compilation.GetState<List<string>>(FunctionConstructorParameters).ToArray(),
                Statements = compilation.GetState<List<CompiledStatement>>(FunctionStatements)
            });

            compilation.State = new State();
        }
    }
}
