using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public interface IParameterModule
    {
        void StartParameterParse(Compilation compilation, Action<Compilation, List<string>> onCompleteParameterParse);
        void StartPropertyParameterParse(Compilation compilation, Action<Compilation, Dictionary<string, string>> onCompleteParameterParse);
    }

    public class ParameterModule : IParameterModule
    {
        public const string Parameters = "ParameterModule.Parameters";
        public const string PropertyParameters = "ParameterModule.PropertyParameters";
        public const string OnCompleteFunction = "ParameterModule.OnCompleteFunction";
        public const string PropertyName = "ParameterModule.PropertyName";
        private readonly TokenParameters parameters = new TokenParameters {
            Separators = new char[] { '(', ')', ',' }
        };
        private readonly TokenParameters propertyParameters = new TokenParameters {
            Separators = new char[] { '(', ')', ':', '[', ']', ',' },
            TokenGroups = new List<TokenGroup> { new TokenGroup('(', ')') }
        };

        public void StartParameterParse(Compilation compilation, Action<Compilation, List<string>> onCompleteParameterParse)
        {
            compilation.AddState(OnCompleteFunction, onCompleteParameterParse);
            compilation.AddState(Parameters, new List<string>());
            compilation.TokenParameters.Push(parameters);
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

        public void StartPropertyParameterParse(Compilation compilation, Action<Compilation, Dictionary<string, string>> onCompleteParameterParse)
        {
            compilation.AddState(OnCompleteFunction, onCompleteParameterParse);
            compilation.AddState(PropertyParameters, new Dictionary<string, string>());
            compilation.TokenParameters.Push(propertyParameters);
            compilation.TokenHandler = ExpectPropertyParameters;
        }

        private void ExpectPropertyParameters(Compilation compilation, string token)
        {
            if (token.Trim().Length == 0) {
            } else if (token.Is(")")) {
                compilation.TokenParameters.Pop();
                compilation.GetState<Action<Compilation, Dictionary<string, string>>>(OnCompleteFunction).Invoke(compilation, compilation.GetState<Dictionary<string, string>>(PropertyParameters));
            } else if (token.Is("[") || token.Is(",")) {
                compilation.TokenHandler = ExpectPropertyName;
            } else if (token.Is("]")) {
            } else {
                throw new Exception($"Expected [ or ) but got {token}");
            }
        }

        private void ExpectPropertyName(Compilation compilation, string token)
        {
            compilation.AddState(PropertyName, token.Trim());
            compilation.TokenHandler = ExpectPropertySeparator;
        }

        private void ExpectPropertySeparator(Compilation compilation, string token)
        {
            if (token.Is(":")) {
                compilation.TokenHandler = ExpectPropertyValue;
            } else {
                throw new Exception($"Expected : but got {token} after property name {compilation.GetState(PropertyName)}");
            }
        }

        private void ExpectPropertyValue(Compilation compilation, string token)
        {
            var propertyParameters = compilation.GetState<Dictionary<string, string>>(PropertyParameters);
            propertyParameters[compilation.GetState(PropertyName)] = token;
            compilation.RemoveState(PropertyName);
            compilation.TokenHandler = ExpectPropertyParameters;
        }
    }
}
