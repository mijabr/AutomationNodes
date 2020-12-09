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
        void ExpectCommentEnd(Compilation compilation, string token);
        void ExpectWaitFunctionParameters(Compilation compilation, string token);
        void ScanAssemblyForNodes(Compilation compilation, Assembly assembly);
    }

    public class OpeningModule : IOpeningModule
    {
        private readonly IConstructionModule constructionModule;
        private readonly ISetFunctionModule setFunctionModule;
        private readonly ITransitionFunctionModule transitionFunctionModule;
        private readonly IClassModule classModule;

        public OpeningModule(
            IConstructionModule constructionModule,
            ISetFunctionModule setFunctionModule,
            ITransitionFunctionModule transitionFunctionModule,
            IClassModule classModule)
        {
            this.constructionModule = constructionModule;
            this.setFunctionModule = setFunctionModule;
            this.transitionFunctionModule = transitionFunctionModule;
            this.classModule = classModule;
        }

        private const string OpeningToken = "OpeningToken";
        private const string FunctionName = "FunctionName";

        public void ExpectNothingInParticular(Compilation compilation, string token)
        {
            if (token.Is(";")) {
                compilation.State = new State();
            } else  if (token.Is("@")) {
                compilation.Expecting = ExpectOpenBracketForAt;
            } else if (token.IsKeyword("using")) {
                compilation.Expecting = ExpectUsing;
            } else if (token.IsKeyword("var")) {
                compilation.Expecting = constructionModule.ExpectVarName;
            } else if (token.IsKeyword("class")) {
                compilation.Expecting = classModule.ExpectClassName;
            } else if (token == ".") {
                compilation.State = new State(compilation.State.Variable);
                compilation.Expecting = ExpectFunctionName;
            } else if (token == "//") {
                compilation.Expecting = ExpectCommentEnd;
            } else {
                compilation.AddState(OpeningToken, token);
                compilation.Expecting = ExpectConstructorOpenBracketOrDot;
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
            compilation.Expecting = ExpectNothingInParticular;
        }

        private void ExpectConstructorOpenBracketOrDot(Compilation compilation, string token)
        {
            var current = compilation.State;

            if (token.Is("(")) {
                if (current.Variable == null) {
                    current.Variable = new Variable { Name = Guid.NewGuid().ToString() };
                    compilation.Variables.Add(current.Variable.Name, current.Variable);
                }
                constructionModule.ExpectTypeName(compilation, compilation.GetState(OpeningToken));
                constructionModule.ExpectOpenBracket(compilation, token);
            } else if (token.Is(".")) {
                current.Variable = compilation.Variables[compilation.GetState(OpeningToken)];
                compilation.Expecting = ExpectFunctionName;
            } else {
                throw new Exception($"Expected . or ( after {compilation.GetState(OpeningToken)}");
            }
        }

        private void ExpectOpenBracketForAt(Compilation compilation, string token)
        {
            if (token.Is("(")) {
                compilation.Expecting = ExpectAtParameter;
            } else {
                throw new Exception($"Expected ( but got {token}");
            }
        }

        private void ExpectAtParameter(Compilation compilation, string token)
        {
            compilation.SceneTime = token.ToTimeSpan();
            compilation.Expecting = ExpectCloseBracket;
        }

        private void ExpectCloseBracket(Compilation compilation, string token)
        {
            if (token.Is(")")) {
                compilation.Expecting = ExpectNothingInParticular;
            } else {
                throw new Exception($"Expected ) but got {token}");
            }
        }

        public void ExpectCommentEnd(Compilation compilation, string token)
        {
            if (token.Is("\r")) {
                compilation.Expecting = ExpectNothingInParticular;
            }
        }

        private void ExpectFunctionName(Compilation compilation, string token)
        {
            compilation.AddState(FunctionName, token);
            compilation.Expecting = ExpectFunctionOpenBracket;
        }

        public void ExpectFunctionOpenBracket(Compilation compilation, string token)
        {
            if (!token.Is("(")) {
                throw new Exception($"Expected ( after {compilation.GetState(FunctionName)}");
            }
            if (compilation.IsState(FunctionName, "set")) {
                compilation.Expecting = setFunctionModule.ExpectSetFunctionParameters;
            } else if (compilation.IsState(FunctionName, "transition")) {
                transitionFunctionModule.ExpectOpenBraket(compilation, token);
            } else if (compilation.IsState(FunctionName, "wait")) {
                compilation.Expecting = ExpectWaitFunctionParameters;
            } else {
                throw new Exception($"Unknown function {compilation.GetState(FunctionName)}");
            }
        }

        public void ExpectWaitFunctionParameters(Compilation compilation, string token)
        {
            compilation.State.Variable.Duration += token.ToTimeSpan();
            compilation.Expecting = ExpectCloseBracket;
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
