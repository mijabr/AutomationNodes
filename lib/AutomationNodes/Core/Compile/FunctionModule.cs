using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public interface IFunctionModule
    {
        void ExpectFunctionName(Compilation compilation, string token);
        void CompileFunctionInstance(Compilation compilation, Function function, List<string> parameterValues);
    }

    public class FunctionModule : IFunctionModule
    {
        private readonly Lazy<IOpeningModule> openingModule;
        private readonly Lazy<IConstructionModule> constructionModule;
        private readonly Lazy<ISetFunctionModule> setFunctionModule;
        private readonly Lazy<ITransitionFunctionModule> transitionFunctionModule;

        public FunctionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
            constructionModule = new Lazy<IConstructionModule>(() => (IConstructionModule)serviceProvider.GetService(typeof(IConstructionModule)));
            setFunctionModule = new Lazy<ISetFunctionModule>(() => (ISetFunctionModule)serviceProvider.GetService(typeof(ISetFunctionModule)));
            transitionFunctionModule = new Lazy<ITransitionFunctionModule>(() => (ITransitionFunctionModule)serviceProvider.GetService(typeof(ITransitionFunctionModule)));
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
            compilation.IsCompilingADefinition = true;
        }

        private void ExpectFunctionDefinition(Compilation compilation, string token)
        {
            if (token.Is("}")) {
                compilation.States.Pop();
                compilation.AddState(FunctionStatements, compilation.StatementsOutput.Pop());
                CompileStatement(compilation);
                compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
                compilation.IsCompilingADefinition = false;
            }
        }

        private static void CompileStatement(Compilation compilation)
        {
            var functionName = compilation.GetState(FunctionName);
            compilation.Functions.Add(functionName, new Function {
                FunctionName = functionName,
                ConstructorParameters = compilation.GetState<List<string>>(FunctionConstructorParameters).ToArray(),
                Statements = compilation.GetState<List<CompiledStatement>>(FunctionStatements)
            });

            compilation.State = new State();
        }

        public void CompileFunctionInstance(Compilation compilation, Function function, List<string> parameterValues)
        {
            var functionScope = Guid.NewGuid().ToString();
            var functionParameters = function.ConstructorParameters
                .Select((p, index) => new KeyValuePair<string, string>($"%{p}%", index < parameterValues.Count ? parameterValues[index] : string.Empty))
                .ToDictionary();

            foreach (var statement in function.Statements) {
                switch (statement) {
                    case SceneCreateStatement createStatement: constructionModule.Value.CompileInstanceStatement(compilation, createStatement, functionParameters, functionScope); break;
                    case SceneSetPropertyStatement setStatement: setFunctionModule.Value.CompileInstanceStatement(compilation, setStatement, functionParameters); break;
                    case SceneSetTransitionStatement transitionStatement: transitionFunctionModule.Value.CompileInstanceStatement(compilation, transitionStatement, functionParameters); break;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}
