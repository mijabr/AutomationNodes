using System;

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

        public void ExpectClassName(Compilation compilation, string token)
        {
            compilation.AddState(ClassName, token);
            compilation.CompileToken = ExpectOpenBracket;
        }

        private void ExpectOpenBracket(Compilation compilation, string token)
        {
            if (token.Is("(")) {
                compilation.CompileToken = ExpectingConstructorParameters;
            } else {
                throw new Exception($"Expected ( but got {token}");
            }
        }

        private void ExpectingConstructorParameters(Compilation compilation, string token)
        {
            if (token.Is(")")) {
                compilation.CompileToken = ExpectOpenBrace;
            }
        }

        private void ExpectOpenBrace(Compilation compilation, string token)
        {
            if (!token.Is("{")) {
                throw new Exception($"Expected ( but got {token}");
            }

            compilation.CompileToken = ExpectClassDefinition;
        }

        private void ExpectClassDefinition(Compilation compilation, string token)
        {
            if (token.Is("}")) {
                CompileStatement(compilation);
                compilation.CompileToken = openingModule.Value.ExpectNothingInParticular;
            }
        }

        private void CompileStatement(Compilation compilation)
        {
            compilation.CompiledStatements.Add(new SceneClassStatement {
                ClassName = compilation.GetState(ClassName)
            }); ;

            compilation.State = new State();
        }
    }
}
