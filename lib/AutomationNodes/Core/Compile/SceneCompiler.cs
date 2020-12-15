using System.Collections.Generic;
using System.Reflection;

namespace AutomationNodes.Core.Compile
{
    public interface ISceneCompiler
    {
        List<CompiledStatement> Compile(string script);
    }

    public class SceneCompiler : ISceneCompiler
    {
        private readonly IScriptTokenizer scriptTokenizer;
        private readonly IOpeningModule openingModule;

        public SceneCompiler(
            IScriptTokenizer scriptTokenizer,
            IOpeningModule openingModule)
        {
            this.scriptTokenizer = scriptTokenizer;
            this.openingModule = openingModule;
        }

        public List<CompiledStatement> Compile(string script)
        {
            var compilation = new Compilation(openingModule.ExpectNothingInParticular, tokenParameters);

            openingModule.ScanAssemblyForNodes(compilation, Assembly.GetExecutingAssembly());

            var tokenContext = scriptTokenizer.Tokenize(script);
            var token = tokenContext.NextToken(compilation.TokenParameters.Peek());
            while (token != null) {
                compilation.TokenHandler(compilation, token);
                token = tokenContext.NextToken(compilation.TokenParameters.Peek());
            }

            return compilation.Statements;
        }

        private readonly TokenParameters tokenParameters = new TokenParameters {
            SplitByWhitespace = true,
            Separators = new char[] { '(', '=', ';', '.', ')' },
            TokenStrings = new string[] { "//" }
        };
    }
}
