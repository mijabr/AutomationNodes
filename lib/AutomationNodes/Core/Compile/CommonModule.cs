using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AutomationNodes.Core.Compile
{
    public interface ICommonModule
    {
        void ExpectNothingInParticular(Compilation compilation, string token);
        void ExpectUsing(Compilation compilation, string token);
        void ExpectAtParameter(Compilation compilation, string token);
        void ExpectCloseBracket(Compilation compilation, string token);
        void ExpectCommentEnd(Compilation compilation, string token);
        void ExpectFunctionName(Compilation compilation, string token);
        void ExpectFunctionOpenBracket(Compilation compilation, string token);
        void ExpectWaitFunctionParameters(Compilation compilation, string token);
        void ScanAssemblyForNodes(Compilation compilation, Assembly assembly);
        void CompileStatement(Compilation compilation);
    }

    public class CommonModule : ICommonModule
    {
        private readonly IConstructionModule constructionModule;
        private readonly ISetFunctionModule setFunctionModule;
        private readonly ITransitionFunctionModule transitionFunctionModule;

        public CommonModule(
            IConstructionModule constructionModule,
            ISetFunctionModule setFunctionModule,
            ITransitionFunctionModule transitionFunctionModule)
        {
            this.constructionModule = constructionModule;
            this.setFunctionModule = setFunctionModule;
            this.transitionFunctionModule = transitionFunctionModule;
        }

        public void ExpectNothingInParticular(Compilation compilation, string token)
        {
            if (token == ";") {
                CompileStatement(compilation);
                compilation.CurrentStatement = new Statement();
                return;
            }

            if (token.IsKeyword("using")) {
                compilation.Expecting = ExpectUsing;
                return;
            }

            if (token.IsKeyword("var")) {
                compilation.Expecting = constructionModule.ExpectVar;
                return;
            }

            var current = compilation.CurrentStatement;
            if (token == ".") {
                CompileStatement(compilation);
                compilation.CurrentStatement = new Statement();
                compilation.CurrentStatement.Variable = current.Variable;
                compilation.Expecting = ExpectFunctionName;
                return;
            }

            if (token == "//") {
                compilation.Expecting = ExpectCommentEnd;
                return;
            }

            if (current.Token == null) {
                current.Token = token;
                compilation.Expecting = constructionModule.ExpectConstructorOpenBracketOrDotOrAt;
                return;
            }

            throw new Exception($"What is {token}");
        }

        public void ExpectUsing(Compilation compilation, string token)
        {
            try {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                ScanAssemblyForNodes(compilation, Assembly.LoadFrom($"{path}\\{token}"));
            } catch (FileNotFoundException x) {
                ScanAssemblyForNodes(compilation, Assembly.LoadFrom(token));
            }
            compilation.Expecting = ExpectNothingInParticular;
        }

        public void ExpectAtParameter(Compilation compilation, string token)
        {
            compilation.SceneTime = token.ToTimeSpan();
            compilation.Expecting = ExpectCloseBracket;
            return;
        }

        public void ExpectCloseBracket(Compilation compilation, string token)
        {
            if (token != ")") {
                throw new Exception($"Expected ) but got {token}");
            }
            compilation.Expecting = ExpectNothingInParticular;
            return;
        }

        public void ExpectCommentEnd(Compilation compilation, string token)
        {
            if (token == "\r") {
                compilation.Expecting = ExpectNothingInParticular;
            }
            return;
        }

        public void ExpectFunctionName(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;
            if (current.FunctionName == null) {
                current.FunctionName = token;
                compilation.Expecting = ExpectFunctionOpenBracket;
            }
            return;
        }

        public void ExpectFunctionOpenBracket(Compilation compilation, string token)
        {
            var current = compilation.CurrentStatement;
            if (token != "(") {
                throw new Exception($"Expected ( after {current.FunctionName}");
            }
            if (string.Equals(current.FunctionName, "set", StringComparison.InvariantCultureIgnoreCase)) {
                compilation.Expecting = setFunctionModule.ExpectSetFunctionParameters;
                return;
            } else if (string.Equals(current.FunctionName, "transition", StringComparison.InvariantCultureIgnoreCase)) {
                compilation.Expecting = transitionFunctionModule.ExpectTransitionFunctionParameters;
                return;
            } else if (string.Equals(current.FunctionName, "wait", StringComparison.InvariantCultureIgnoreCase)) {
                compilation.Expecting = ExpectWaitFunctionParameters;
                return;
            }
            throw new Exception($"Unknown function {current.FunctionName}");
        }

        public void ExpectWaitFunctionParameters(Compilation compilation, string token)
        {
            compilation.CurrentStatement.Variable.Duration += token.ToTimeSpan();
            compilation.Expecting = ExpectCloseBracket;
            return;
        }

        public void ScanAssemblyForNodes(Compilation compilation, Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in assembly.GetTypes().Where(t => typeof(INode).IsAssignableFrom(t))) {
                compilation.TypesLibrary.Add(type.Name, type);
            }
        }

        public void CompileStatement(Compilation compilation)
        {
            var current = compilation.CurrentStatement;

            if (current.TypeName != null) {
                if (!compilation.TypesLibrary.TryGetValue(current.TypeName, out var type)) {
                    throw new Exception($"Unknown node type '{current.TypeName}'. Are you missing a using?");
                }

                compilation.Statements.Add(new SceneCreateStatement {
                    TriggerAt = compilation.SceneTime,
                    NodeName = current.Variable.Name,
                    Type = type,
                    Parameters = current.Parameter.ToArray()
                });

                return;
            }

            if (current.SetFunctionParameterName != null) {
                compilation.Statements.Add(new SceneSetPropertyEvent {
                    TriggerAt = compilation.SceneTime + current.Variable.Duration,
                    NodeName = current.Variable.Name,
                    PropertyName = current.SetFunctionParameterName,
                    PropertyValue = current.SetFunctionParameterValue
                });

                return;
            }

            if (current.TransitionParameters != null) {
                var duration = TimeSpan.FromMilliseconds(int.Parse(current.Duration));
                compilation.Statements.Add(new SceneSetTransitionEvent {
                    TriggerAt = compilation.SceneTime + current.Variable.Duration,
                    NodeName = current.Variable.Name,
                    TransitionProperties = current.TransitionParameters,
                    Duration = duration
                });

                current.Variable.Duration += duration;
                return;
            }
        }
    }
}
