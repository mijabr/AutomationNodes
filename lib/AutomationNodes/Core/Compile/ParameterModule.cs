using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public interface IParameterModule
    {
        void StartParameterParse(Compilation compilation, Action<Compilation, List<string>> onCompleteParameterParse);
    }

    public class ParameterModule : IParameterModule
    {
        public const string Parameters = "ParameterModule.Parameters";
        public const string OnCompleteFunction = "ParameterModule.OnCompleteFunction";
        private readonly TokenParameters constructorTokenParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ',' }
        };

        public void StartParameterParse(Compilation compilation, Action<Compilation, List<string>> onCompleteParameterParse)
        {
            compilation.AddState(OnCompleteFunction, onCompleteParameterParse);
            compilation.AddState(Parameters, new List<string>());
            compilation.TokenParameters.Push(constructorTokenParameters);
            compilation.TokenHandler = ExpectParameters;
        }

        private void ExpectParameters(Compilation compilation, string token)
        {
            if (token == ")") {
                compilation.TokenParameters.Pop();
                compilation.GetState<Action<Compilation, List<string>>>(OnCompleteFunction).Invoke(compilation, compilation.GetState<List<string>>(Parameters));
            } else if (token != ",") {
                compilation.GetState<List<string>>(Parameters).Add(token.Trim());
            }
        }
    }
}
