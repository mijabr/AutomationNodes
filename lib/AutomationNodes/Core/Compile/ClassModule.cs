using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public interface IClassModule
    {
        void ExpectClassName(Compilation compilation, string token);
        void StartInstanceParameterParse(Compilation compilation, Class theClass);
    }

    public class ClassModule : IClassModule
    {
        private readonly Lazy<IOpeningModule> openingModule;
        private readonly Lazy<IParameterModule> parameterModule;
        private readonly Lazy<IConstructionModule> constructionModule;
        private readonly Lazy<ISetFunctionModule> setFunctionModule;
        private readonly Lazy<ITransitionFunctionModule> transitionFunctionModule;

        public ClassModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
            parameterModule = new Lazy<IParameterModule>(() => (IParameterModule)serviceProvider.GetService(typeof(IParameterModule)));
            constructionModule = new Lazy<IConstructionModule>(() => (IConstructionModule)serviceProvider.GetService(typeof(IConstructionModule)));
            setFunctionModule = new Lazy<ISetFunctionModule>(() => (ISetFunctionModule)serviceProvider.GetService(typeof(ISetFunctionModule)));
            transitionFunctionModule = new Lazy<ITransitionFunctionModule>(() => (ITransitionFunctionModule)serviceProvider.GetService(typeof(ITransitionFunctionModule)));
        }

        private const string ClassName = "ClassModule.ClassName";
        private const string ConstructorParameters = "ClassModule.ConstructorParameters";
        private const string Statements = "ClassModule.Statements";
        private const string ClassInstance = "ClassModule.ClassInstance";

        public void ExpectClassName(Compilation compilation, string token)
        {
            compilation.AddState(ClassName, token);
            compilation.TokenHandler = ExpectOpenBracket;
        }

        private void ExpectOpenBracket(Compilation compilation, string token)
        {
            if (token.Is("(")) {
                parameterModule.Value.StartParameterParse(compilation, OnCompleteParameterParse);
            } else {
                throw new Exception($"Expected ( but got {token}");
            }
        }

        private void OnCompleteParameterParse(Compilation compilation, List<string> parameters)
        {
            compilation.AddState<List<string>>(ConstructorParameters, parameters);
            compilation.TokenHandler = ExpectOpenBrace;
        }

        private void ExpectOpenBrace(Compilation compilation, string token)
        {
            if (!token.Is("{")) {
                throw new Exception($"Expected {{ but got {token}");
            }

            compilation.States.Push(new State());
            compilation.StatementsOutput.Push(new List<CompiledStatement>());
            compilation.TokenHandler = ExpectClassDefinition;
            compilation.TokenHandlers.Push(openingModule.Value.ExpectNothingInParticular);
        }

        private void ExpectClassDefinition(Compilation compilation, string token)
        {
            if (token.Is("}")) {
                compilation.States.Pop();
                compilation.AddState(Statements, compilation.StatementsOutput.Pop());
                CompileStatement(compilation);
                compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
            }
        }

        private static void CompileStatement(Compilation compilation)
        {
            var className = compilation.GetState(ClassName);
            compilation.Classes.Add(className, new Class {
                ClassName = className,
                ConstructorParameters = compilation.GetState<List<string>>(ConstructorParameters).ToArray(),
                Statements = compilation.GetState<List<CompiledStatement>>(Statements)
            });

            compilation.State = new State();
        }

        public void StartInstanceParameterParse(Compilation compilation, Class theClass)
        {
            compilation.AddState(ClassInstance, theClass);
            parameterModule.Value.StartParameterParse(compilation, OnCompleteInstanceParameterParse);
        }

        private void OnCompleteInstanceParameterParse(Compilation compilation, List<string> parameters)
        {
            CompileClassInstance(compilation, parameters);
            compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
        }

        public void CompileClassInstance(Compilation compilation, List<string> parameterValues)
        {
            if (compilation.State.Variable == null) {
                compilation.NewVariable(Guid.NewGuid().ToString());
            }

            var theClass = compilation.GetState<Class>(ClassInstance);
            var variableName = compilation.State.Variable.Fullname;

            compilation.StatementsOutput.Peek().Add(new SceneCreateStatement {
                TriggerAt = compilation.SceneTime,
                NodeName = variableName,
                Class = theClass.ClassName,
                Type = typeof(GenericNode),
                Parameters = parameterValues.ToArray()
            });

            var classParameters = theClass.ConstructorParameters
                .Select((p, index) => new KeyValuePair<string, string>($"%{p}%", index < parameterValues.Count ? parameterValues[index] : string.Empty))
                .ToDictionary();

            compilation.States.Push(new State());

            foreach (var statement in theClass.Statements) {
                switch (statement) {
                    case SceneCreateStatement createStatement: constructionModule.Value.CompileInstanceStatement(compilation, createStatement, classParameters, variableName, variableName); break;
                    case SceneSetPropertyStatement setStatement: setFunctionModule.Value.CompileInstanceStatement(compilation, setStatement, classParameters); break;
                    case SceneSetTransitionStatement transitionStatement: transitionFunctionModule.Value.CompileInstanceStatement(compilation, transitionStatement, classParameters); break;
                    default: throw new NotImplementedException();
                }
            }

            compilation.States.Pop();
        }
    }
}
