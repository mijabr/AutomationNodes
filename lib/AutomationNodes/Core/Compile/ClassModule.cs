using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public interface IClassModule
    {
        void ExpectClassName(Compilation compilation, string token);
        void CompileClassInstance(Compilation compilation, Class nodeClass, List<string> parameterValues);
    }

    public class ClassModule : IClassModule
    {
        private readonly Lazy<IOpeningModule> openingModule;
        private readonly Lazy<IConstructionModule> constructionModule;
        private readonly Lazy<ISetFunctionModule> setFunctionModule;
        private readonly Lazy<ITransitionFunctionModule> transitionFunctionModule;

        public ClassModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
            constructionModule = new Lazy<IConstructionModule>(() => (IConstructionModule)serviceProvider.GetService(typeof(IConstructionModule)));
            setFunctionModule = new Lazy<ISetFunctionModule>(() => (ISetFunctionModule)serviceProvider.GetService(typeof(ISetFunctionModule)));
            transitionFunctionModule = new Lazy<ITransitionFunctionModule>(() => (ITransitionFunctionModule)serviceProvider.GetService(typeof(ITransitionFunctionModule)));
        }

        private const string ClassName = "ClassName";
        private const string ConstructorParameters = "ConstructorParameters";
        private const string Statements = "Statements";
        private readonly TokenParameters constructorTokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ',' }
        };

        public void ExpectClassName(Compilation compilation, string token)
        {
            compilation.AddState(ClassName, token);
            compilation.TokenHandler = ExpectOpenBracket;
        }

        private void ExpectOpenBracket(Compilation compilation, string token)
        {
            if (token.Is("(")) {
                compilation.AddState<List<string>>(ConstructorParameters, new List<string>());
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
                compilation.GetState<List<string>>(ConstructorParameters).Add(token.Trim());
            }
        }

        private void ExpectOpenBrace(Compilation compilation, string token)
        {
            if (!token.Is("{")) {
                throw new Exception($"Expected ( but got {token}");
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

        public void CompileClassInstance(Compilation compilation, Class nodeClass, List<string> parameterValues)
        {
            var classScope = compilation.State.Variable.Fullname;
            var classParameters = nodeClass.ConstructorParameters
                .Select((p, index) => new KeyValuePair<string, string>($"%{p}%", index < parameterValues.Count ? parameterValues[index] : string.Empty))
                .ToDictionary();

            compilation.States.Push(new State());

            foreach (var statement in nodeClass.Statements) {
                switch (statement) {
                    case SceneCreateStatement createStatement: constructionModule.Value.CompileInstanceStatement(compilation, createStatement, classParameters, classScope, classScope); break;
                    case SceneSetPropertyStatement setStatement: setFunctionModule.Value.CompileInstanceStatement(compilation, setStatement, classParameters); break;
                    case SceneSetTransitionStatement transitionStatement: transitionFunctionModule.Value.CompileInstanceStatement(compilation, transitionStatement, classParameters); break;
                    default: throw new NotImplementedException();
                }
            }

            compilation.States.Pop();
        }
    }
}
