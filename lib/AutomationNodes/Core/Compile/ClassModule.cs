using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public interface IClassModule
    {
        void ExpectClassName(Compilation compilation, string token);
    }

    public class ClassModule : IClassModule
    {
        private readonly Lazy<IOpeningModule> openingModule;

        public ClassModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
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

        private void CompileStatement(Compilation compilation)
        {
            var className = compilation.GetState(ClassName);
            compilation.Classes.Add(className, new Class {
                ClassName = className,
                ConstructorParameters = compilation.GetState<List<string>>(ConstructorParameters).ToArray(),
                Statements = compilation.GetState<List<CompiledStatement>>(Statements)
            });

            compilation.State = new State();
        }
    }
}
