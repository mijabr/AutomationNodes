using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public interface IKeyframeModule
    {
        void ExpectOpenBraket(Compilation compilation, string token);
        void CompileInstanceStatement(Compilation compilation, SceneKeyframeStatement keyframeStatement, Dictionary<string, string> parameters);
    }

    public class KeyframeModule : IKeyframeModule
    {
        private readonly Lazy<IOpeningModule> openingModule;
        private readonly Lazy<IParameterModule> parameterModule;

        public KeyframeModule(IServiceProvider serviceProvider)
        {
            openingModule = new Lazy<IOpeningModule>(() => (IOpeningModule)serviceProvider.GetService(typeof(IOpeningModule)));
            parameterModule = new Lazy<IParameterModule>(() => (IParameterModule)serviceProvider.GetService(typeof(IParameterModule)));
        }

        public void ExpectOpenBraket(Compilation compilation, string token)
        {
            if (!token.Is("(")) {
                throw new Exception($"Expected ( but got {token}");
            }

            parameterModule.Value.StartPropertyParameterParse(compilation, OnParameterParseComplete);
        }

        private void OnParameterParseComplete(Compilation compilation, Dictionary<string, string> propertyParameters)
        {
            var keyframeName = propertyParameters["keyframe-name"];
            var keyframePercent = propertyParameters["keyframe-percent"];
            propertyParameters.Remove("keyframe-name");
            propertyParameters.Remove("keyframe-percent");
            CompileStatement(compilation, propertyParameters, keyframeName, keyframePercent);
            compilation.State = new State(compilation.State.Variable);
            compilation.TokenHandler = openingModule.Value.ExpectNothingInParticular;
        }

        public void CompileInstanceStatement(Compilation compilation, SceneKeyframeStatement keyframeStatement, Dictionary<string, string> parameters)
        {
            var variable = compilation.State.Variable;
            compilation.State = new State();
            compilation.State.Variable = variable;
            if (compilation.State.Variable == null || compilation.State.Variable.Name != keyframeStatement.NodeName) {
                compilation.State.Variable = compilation.Variables[keyframeStatement.NodeName];
            }
            var keyframeParameters = keyframeStatement.KeyframeProperties
                .Select(p => new KeyValuePair<string, string>(p.Key, parameters.TryGetValue(p.Value.Trim(), out var value) ? value : p.Value))
                .ToDictionary();
            CompileStatement(compilation, keyframeParameters, keyframeStatement.KeyframeName, keyframeStatement.KeyframePercent.ReplaceTokenUsingParameters(parameters));
        }

        private static void CompileStatement(Compilation compilation, Dictionary<string, string> keyframeParameters, string keyframeName, string keyframePercent)
        {
            compilation.StatementsOutput.Peek().Add(new SceneKeyframeStatement {
                TriggerAt = compilation.SceneTime,
                KeyframeProperties = keyframeParameters,
                KeyframeName = keyframeName,
                KeyframePercent = keyframePercent
            });
        }
    }
}
