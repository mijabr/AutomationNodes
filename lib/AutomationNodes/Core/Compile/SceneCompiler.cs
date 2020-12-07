using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutomationNodes.Core.Compile
{
    public class SceneStatement
    {
        public TimeSpan TriggerAt { get; set; }
        public string NodeName { get; set; }
    }

    public class SceneCreateStatement : SceneStatement
    {
        public Type Type { get; set; }
        public string[] Parameters { get; set; }
    }

    public class SceneSetPropertyEvent : SceneStatement
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

    public class SceneSetTransitionEvent : SceneStatement
    {
        public Dictionary<string, string> TransitionProperties { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public interface ISceneCompiler
    {
        List<SceneStatement> Compile(string script);
    }

    public class SceneCompiler : ISceneCompiler
    {
        private readonly IScriptTokenizer scriptTokenizer;
        private readonly ICommonModule commonModule;
        private readonly IConstructionModule constructionModule;
        private readonly ISetFunctionModule setFunctionModule;
        private readonly ITransitionFunctionModule transitionFunctionModule;

        public SceneCompiler(
            IScriptTokenizer scriptTokenizer,
            ICommonModule commonModule,
            IConstructionModule constructionModule,
            ISetFunctionModule setFunctionModule,
            ITransitionFunctionModule transitionFunctionModule)
        {
            this.scriptTokenizer = scriptTokenizer;
            this.commonModule = commonModule;
            this.constructionModule = constructionModule;
            this.setFunctionModule = setFunctionModule;
            this.transitionFunctionModule = transitionFunctionModule;
        }


        public List<SceneStatement> Compile(string script)
        {
            var compilation = new Compilation();

            commonModule.ScanAssemblyForNodes(compilation, Assembly.GetExecutingAssembly());

            var tokenContext = scriptTokenizer.Tokenize(script);
            var token = GetToken(compilation, tokenContext);
            while (token != null) {
                ProcessToken(compilation, token);
                token = GetToken(compilation, tokenContext);
            }

            return compilation.Statements;
        }

        private TokenParameters tokenParameters = new TokenParameters {
            SplitByWhitespace = true,
            Separators = new char[] { '(', '=', ';', '.', ')' },
            TokenStrings = new string[] { "//" }

        };

        private TokenParameters constructorTokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ',' }
        };

        private TokenParameters setFunctiontokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ':', '[', ']', ',' },
            TokenGroups = new List<TokenGroup> { new TokenGroup('(', ')') }
        };

        private TokenParameters commentParameters = new TokenParameters {
            Separators = new[] { '\r', '\n' }
        };

        private string GetToken(Compilation compilation, ITokenContext context)
        {
            if (compilation.Expecting == constructionModule.ExpectConstructorParameters) {
                return context.NextToken(constructorTokenParameters);
            }

            if (compilation.Expecting == setFunctionModule.ExpectSetFunctionParameters ||
                compilation.Expecting == setFunctionModule.ExpectSetFunctionParameterPropertyName ||
                compilation.Expecting == setFunctionModule.ExpectSetFunctionParameterPropertySeparator ||
                compilation.Expecting == setFunctionModule.ExpectSetFunctionParameterPropertyValue) {
                return context.NextToken(setFunctiontokenParameters);
            }

            if (compilation.Expecting == transitionFunctionModule.ExpectTransitionFunctionParameters ||
                compilation.Expecting == transitionFunctionModule.ExpectTransitionFunctionParameterPropertyName ||
                compilation.Expecting == transitionFunctionModule.ExpectTransitionFunctionParameterPropertySeparator ||
                compilation.Expecting == transitionFunctionModule.ExpectTransitionFunctionParameterPropertyValue) {
                return context.NextToken(setFunctiontokenParameters);
            }

            if (compilation.Expecting == commonModule.ExpectCommentEnd) {
                return context.NextToken(commentParameters);
            }

            return context.NextToken(tokenParameters);
        }

        private void ProcessToken(Compilation compilation, string token)
        {
            if (compilation.Expecting == null) {
                compilation.Expecting = commonModule.ExpectNothingInParticular;
            }
 
            compilation.Expecting(compilation, token);
        }
    }
}
