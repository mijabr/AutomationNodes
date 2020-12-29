using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AutomationNodes.Core.Compile
{
    public interface IOpeningModule
    {
        void ExpectNothingInParticular(Compilation compilation, string token);
        void ExpectFunctionOpenBracket(Compilation compilation, string token);
        void ExpectWaitFunctionParameters(Compilation compilation, string token);
        void ScanAssemblyForNodes(Compilation compilation, Assembly assembly);
    }

    public class OpeningModule : IOpeningModule
    {
        private readonly IConstructionModule constructionModule;
        private readonly ISetFunctionModule setFunctionModule;
        private readonly ITransitionFunctionModule transitionFunctionModule;
        private readonly IClassModule classModule;
        private readonly IFunctionModule functionModule;

        public OpeningModule(
            IConstructionModule constructionModule,
            ISetFunctionModule setFunctionModule,
            ITransitionFunctionModule transitionFunctionModule,
            IClassModule classModule,
            IFunctionModule functionModule)
        {
            this.constructionModule = constructionModule;
            this.setFunctionModule = setFunctionModule;
            this.transitionFunctionModule = transitionFunctionModule;
            this.classModule = classModule;
            this.functionModule = functionModule;
        }

        private const string OpeningToken = "OpeningToken";
        private const string FunctionName = "FunctionName";
        private readonly TokenParameters commentParameters = new TokenParameters {
            Separators = new[] { '\r', '\n' }
        };

        public void ExpectNothingInParticular(Compilation compilation, string token)
        {
            if (token.Is(";")) {
                compilation.State = new State();
            }  else if (token.Is("}")) {
                compilation.TokenHandlers.Pop();
                compilation.TokenHandler(compilation, token);
            } else if (token.Is("@")) {
                compilation.TokenHandler = ExpectOpenBracketForAt;
            } else if (token.IsKeyword("using")) {
                compilation.TokenHandler = ExpectUsing;
            } else if (token.IsKeyword("var")) {
                compilation.TokenHandler = constructionModule.ExpectVarName;
            } else if (token.IsKeyword("function")) {
                compilation.TokenHandler = functionModule.ExpectFunctionName;
            } else if (token.IsKeyword("class")) {
                compilation.TokenHandler = classModule.ExpectClassName;
            } else if (token == ".") {
                compilation.State = new State(compilation.State.Variable);
                compilation.TokenHandler = ExpectFunctionName;
            } else if (token == "//") {
                compilation.TokenParameters.Push(commentParameters);
                compilation.TokenHandler = ExpectCommentEnd;
            } else {
                compilation.AddState(OpeningToken, token);
                compilation.TokenHandler = ExpectConstructorOpenBracketOrDot;
            }
        }

        private void ExpectUsing(Compilation compilation, string token)
        {
            try {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ScanAssemblyForNodes(compilation, Assembly.LoadFrom($"{path}\\{token}"));
            } catch (FileNotFoundException x) {
                ScanAssemblyForNodes(compilation, Assembly.LoadFrom(token));
            }
            compilation.TokenHandler = ExpectNothingInParticular;
        }

        private void ExpectConstructorOpenBracketOrDot(Compilation compilation, string token)
        {
            var current = compilation.State;

            if (token.Is("(")) {
                constructionModule.ExpectTypeName(compilation, compilation.GetState(OpeningToken));
                constructionModule.ExpectOpenBracket(compilation, token);
            } else if (token.Is(".")) {
                current.Variable = compilation.Variables.TryGetValue(compilation.GetState(OpeningToken), out var v) ? v : null;
                if (current.Variable == null) {
                    throw new Exception($"Unknown variable '{compilation.GetState(OpeningToken)}'");
                }
                compilation.TokenHandler = ExpectFunctionName;
            } else {
                throw new Exception($"Expected . or ( after {compilation.GetState(OpeningToken)}");
            }
        }

        private void ExpectOpenBracketForAt(Compilation compilation, string token)
        {
            if (token.Is("(")) {
                compilation.TokenHandler = ExpectAtParameter;
            } else {
                throw new Exception($"Expected ( but got {token}");
            }
        }

        private void ExpectAtParameter(Compilation compilation, string token)
        {
            compilation.SceneTime = token.ToTimeSpan();
            foreach (var variable in compilation.Variables) {
                variable.Value.Duration = TimeSpan.Zero;
            }
            compilation.TokenHandler = ExpectCloseBracket;
        }

        private void ExpectCloseBracket(Compilation compilation, string token)
        {
            if (token.Is(")")) {
                compilation.TokenHandler = ExpectNothingInParticular;
            } else {
                throw new Exception($"Expected ) but got {token}");
            }
        }

        private void ExpectCommentEnd(Compilation compilation, string token)
        {
            if (token.Is("\r")) {
                compilation.TokenParameters.Pop();
                compilation.TokenHandler = ExpectNothingInParticular;
            }
        }

        private void ExpectFunctionName(Compilation compilation, string token)
        {
            compilation.AddState(FunctionName, token);
            compilation.TokenHandler = ExpectFunctionOpenBracket;
        }

        public void ExpectFunctionOpenBracket(Compilation compilation, string token)
        {
            if (!token.Is("(")) {
                throw new Exception($"Expected ( after {compilation.GetState(FunctionName)}");
            }
            if (compilation.IsState(FunctionName, "set")) {
                setFunctionModule.ExpectOpenBraket(compilation, token);
            } else if (compilation.IsState(FunctionName, "transition")) {
                transitionFunctionModule.ExpectOpenBraket(compilation, token);
            } else if (compilation.IsState(FunctionName, "wait")) {
                compilation.TokenHandler = ExpectWaitFunctionParameters;
            } else {
                throw new Exception($"Unknown function {compilation.GetState(FunctionName)}");
            }
        }

        public void ExpectWaitFunctionParameters(Compilation compilation, string token)
        {
            compilation.State.Variable.Duration += token.ToTimeSpan();
            compilation.TokenHandler = ExpectCloseBracket;
        }

        public void ScanAssemblyForNodes(Compilation compilation, Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in assembly.GetTypes().Where(t => typeof(INode).IsAssignableFrom(t))) {
                compilation.TypesLibrary.Add(type.Name, type);
            }
        }
    }
}
