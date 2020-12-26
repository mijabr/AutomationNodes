using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core.Compile
{
    public class Compilation
    {
        public Compilation(Action<Compilation, string> expecting, TokenParameters tokenParameters)
        {
            State = new State();
            TokenHandler = expecting;
            TokenParameters.Push(tokenParameters);
            StatementsOutput.Push(Statements);
        }

        public void AddState<T>(string key, T value) => State.States.Add(key, value);
        public T GetState<T>(string key) => (T)State.States[key];
        public string GetState(string key) => (string)State.States[key];
        internal void RemoveState(string key) => State.States.Remove(key);
        public bool IsState(string key, string value) => ((string)State.States[key]).Is(value);

        public State State
        {
            get => States.Peek();
            set {
                if (States.Count > 0) {
                    States.Pop();
                }
                States.Push(value);
            }
        }

        public Stack<State> States { get; set; } = new();

        public Stack<Action<Compilation, string>> TokenHandlers { get; set; } = new();

        public Action<Compilation, string> TokenHandler
        {
            get => TokenHandlers.Peek();
            set {
                if (TokenHandlers.Count > 0) {
                    TokenHandlers.Pop();
                }
                TokenHandlers.Push(value);
            }
        }

        public Stack<TokenParameters> TokenParameters { get; set; } = new();

        public Stack<List<CompiledStatement>> StatementsOutput { get; set; } = new();

        public TimeSpan SceneTime { get; set; }

        public Dictionary<string, Type> TypesLibrary = new();
        public Dictionary<string, Function> Functions { get; set; } = new();
        public Dictionary<string, Class> Classes = new();
        public Dictionary<string, Variable> Variables { get; } = new();

        public List<CompiledStatement> Statements { get; set; } = new();
        public string ParentNodeName { get; set; }
    }

    public class State
    {
        public State() { }
        public State(Variable variable) { Variable = variable; }

        public Dictionary<string, object> States { get; set; } = new Dictionary<string, object>();
        public Variable Variable { get; set; }
    }

    public class Function
    {
        public string FunctionName { get; set; }
        public string[] ConstructorParameters { get; set; }
        public List<CompiledStatement> Statements { get; set; }
    }

    public class Class
    {
        public string ClassName { get; set; }
        public string[] ConstructorParameters { get; set; }
        public List<CompiledStatement> Statements { get; set; }
    }

    public class Variable
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public abstract class CompiledStatement
    {
        public TimeSpan TriggerAt { get; set; }

        public abstract CompiledStatement GenerateInstanceStatement(string variableName, Dictionary<string, string> parameterValues);
    }

    public abstract class SceneNodeStatement : CompiledStatement
    {
        public string NodeName { get; set; }
    }

    public class SceneCreateStatement : SceneNodeStatement
    {
        public Type Type { get; set; }
        public string Class { get; set; }
        public string[] Parameters { get; set; }
        public string ParentNodeName { get; set; }

        public override CompiledStatement GenerateInstanceStatement(string variableName, Dictionary<string, string> parameterValues)
        {
            return new SceneCreateStatement {
                TriggerAt = TriggerAt,
                NodeName = $"{variableName}.{NodeName}",
                Type = Type,
                Class = Class,
                Parameters = Parameters.Select(p => parameterValues.TryGetValue(p, out var value) ? value : p).ToArray(),
                ParentNodeName = variableName,
            };
        }
    }

    public class SceneSetPropertyStatement : SceneNodeStatement
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }

        public override CompiledStatement GenerateInstanceStatement(string variableName, Dictionary<string, string> parameterValues)
        {
            return new SceneSetPropertyStatement {
                TriggerAt = TriggerAt,
                NodeName = $"{variableName}.{NodeName}",
                PropertyName = PropertyName,
                PropertyValue = parameterValues.TryGetValue(PropertyValue.Trim(), out var value) ? value : PropertyValue
            };
        }
    }

    public class SceneSetTransitionStatement : SceneNodeStatement
    {
        public Dictionary<string, string> TransitionProperties { get; set; }
        public TimeSpan Duration { get; set; }

        public override CompiledStatement GenerateInstanceStatement(string variableName, Dictionary<string, string> parameterValues)
        {
            return new SceneSetTransitionStatement {
                TriggerAt = TriggerAt,
                NodeName = $"{variableName}.{NodeName}",
                TransitionProperties = TransitionProperties.Select(p => new KeyValuePair<string, string>(p.Key, parameterValues.TryGetValue(p.Value.Trim(), out var value) ? value : p.Value)).ToDictionary(),
                Duration = Duration
            };
        }
    }
}
