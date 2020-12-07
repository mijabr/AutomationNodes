using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationNodes.Core.Compile
{
    public class Compilation
    {
        public List<SceneStatement> Statements { get; set; } = new List<SceneStatement>();

        public Statement CurrentStatement { get; set; } = new Statement();

        public Action<Compilation, string> Expecting { get; set; }

        public TimeSpan SceneTime { get; set; } = TimeSpan.Zero;

        public Dictionary<string, Type> TypesLibrary = new Dictionary<string, Type>();

        public Dictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>();
    }

    public class Statement
    {
        public string Token { get; set; }
        public Variable Variable { get; set; }
        public string TypeName { get; set; }
        public List<string> Parameter { get; } = new List<string>();
        public string FunctionName { get; set; }
        public bool ParameterGroup { get; internal set; }
        public string SetFunctionParameterName { get; set; }
        public string SetFunctionParameterValue { get; set; }
        public string TransitionFunctionParameterName { get; set; }
        public string TransitionFunctionParameterValue { get; set; }
        public string Duration { get; set; }
        public Dictionary<string, string> TransitionParameters { get; set; }
    }

    public class Variable
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
