using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public interface IFunctionModule
    {
        void ExpectFunctionName(Compilation compilation, string token);
        void StartInstanceParameterParse(Compilation compilation, Function function, string classVariableName = null);
    }

    public class FunctionModule : IFunctionModule
    {
        private readonly Lazy<IOpeningModule> openingModule;
        private readonly Lazy<IParameterModule> parameterModule;
        private readonly Lazy<IConstructionModule> constructionModule;
        private readonly Lazy<ISetFunctionModule> setFunctionModule;
        private readonly Lazy<ITransitionFunctionModule> transitionFunctionModule;

        public FunctionModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
            parameterModule = new Lazy<IParameterModule>(() => (IParameterModule)serviceProvider.GetService(typeof(IParameterModule)));
            constructionModule = new Lazy<IConstructionModule>(() => (IConstructionModule)serviceProvider.GetService(typeof(IConstructionModule)));
            setFunctionModule = new Lazy<ISetFunctionModule>(() => (ISetFunctionModule)serviceProvider.GetService(typeof(ISetFunctionModule)));
            transitionFunctionModule = new Lazy<ITransitionFunctionModule>(() => (ITransitionFunctionModule)serviceProvider.GetService(typeof(ITransitionFunctionModule)));
        }

        private const string FunctionName = "FunctionModule.FunctionName";
        private const string FunctionConstructorParameters = "FunctionModule.FunctionConstructorParameters";
        private const string FunctionStatements = "FunctionModule.FunctionStatements";
        private const string FunctionInstance = "FunctionModule.FunctionInstance";
        private const string ClassVariableName = "FunctionModule.ClassVariableName";

        public void ExpectFunctionName(Compilation compilation, string token)
        {
            compilation.AddState(FunctionName, token);
            compilation.TokenHandler = ExpectFunctionDefinitionOpenBracket;
        }

        private void ExpectFunctionDefinitionOpenBracket(Compilation compilation, string token)
        {
            if (token.Is("(")) {
                parameterModule.Value.StartParameterParse(compilation, OnCompleteParameterParse);
            } else {
                throw new Exception($"Expected ( but got {token}");
            }
        }

        private void OnCompleteParameterParse(Compilation compilation, List<string> parameters)
        {
            compilation.AddState(FunctionConstructorParameters, parameters);
            compilation.TokenHandler = ExpectOpenBrace;
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
                AddFunctionDefinition(compilation);
                compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
                compilation.IsCompilingADefinition = false;
            }
        }

        private static void AddFunctionDefinition(Compilation compilation)
        {
            var functionName = compilation.GetState(FunctionName);
            compilation.Functions.Add(functionName, new Function {
                FunctionName = functionName,
                ConstructorParameters = compilation.GetState<List<string>>(FunctionConstructorParameters).ToArray(),
                Statements = compilation.GetState<List<CompiledStatement>>(FunctionStatements)
            });

            compilation.State = new State();
        }

        public void StartInstanceParameterParse(Compilation compilation, Function function, string classVariableName = null)
        {
            compilation.AddState(FunctionInstance, function);
            compilation.AddState(ClassVariableName, classVariableName);
            parameterModule.Value.StartParameterParse(compilation, OnCompleteInstanceParameterParse);
        }

        private void OnCompleteInstanceParameterParse(Compilation compilation, List<string> parameters)
        {
            CompileFunctionInstance(compilation, parameters);
            compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
        }

        private void CompileFunctionInstance(Compilation compilation, List<string> parameterValues)
        {
            var function = compilation.GetState<Function>(FunctionInstance);
            var classVariableName = compilation.GetState(ClassVariableName);
            var functionScope = Guid.NewGuid().ToString();
            var functionParameters = function.ConstructorParameters
                .Select((p, index) => new KeyValuePair<string, string>($"%{p}%", index < parameterValues.Count ? parameterValues[index] : string.Empty))
                .ToDictionary();

            foreach (var statement in function.Statements) {
                var s = statement.Clone();
                s.NodeName = new Variable(s.NodeName, classVariableName).Fullname;
                switch (s) {
                    case SceneCreateStatement createStatement: constructionModule.Value.CompileInstanceStatement(compilation, createStatement, functionParameters, functionScope); break;
                    case SceneSetPropertyStatement setStatement: setFunctionModule.Value.CompileInstanceStatement(compilation, setStatement, functionParameters); break;
                    case SceneSetTransitionStatement transitionStatement: transitionFunctionModule.Value.CompileInstanceStatement(compilation, transitionStatement, functionParameters); break;
                    default: throw new NotImplementedException();
                }
            }
        }
    }
}
