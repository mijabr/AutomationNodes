using System;
using System.Collections.Generic;

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

        public Variable NewVariable(string name, string scope = null) {
            var variable = new Variable(name, scope);
            Variables.Add(variable.Fullname, variable);
            State.Variable = variable;
            return variable;
        }
        public Dictionary<string, Variable> Variables { get; } = new();

        public List<CompiledStatement> Statements { get; set; } = new();
        public string ParentNodeName { get; set; }
        public bool IsCompilingADefinition { get; set; }
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
        public Variable(string name, string scope = null)
        {
            Name = name;
            Scope = scope;
            Fullname = scope == null ? name : $"{scope}-{name}";
        }

        public string Scope { get; set; }
        public string Name { get; set; }
        public string Fullname { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public abstract class CompiledStatement
    {
        public TimeSpan TriggerAt { get; set; }
        public string NodeName { get; set; }
    }

    public class SceneCreateStatement : CompiledStatement
    {
        public Type Type { get; set; }
        public string Class { get; set; }
        public string[] Parameters { get; set; }
        public string ParentNodeName { get; set; }
    }

    public class SceneSetPropertyStatement : CompiledStatement
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

    public class SceneSetTransitionStatement : CompiledStatement
    {
        public Dictionary<string, string> TransitionProperties { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
