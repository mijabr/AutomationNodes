using System;
using System.Collections.Generic;

namespace AutomationNodes.Core.Compile
{
    public class Compilation
    {
        public void AddState<T>(string key, T value) => State.States.Add(key, value);
        public T GetState<T>(string key) => (T)State.States[key];
        public string GetState(string key) => (string)State.States[key];
        internal void RemoveState(string key) => State.States.Remove(key);
        public bool IsState(string key, string value) => ((string)State.States[key]).Is(value);

        public State State { get; set; } = new State();

        public Action<Compilation, string> Expecting { get; set; }

        public TimeSpan SceneTime { get; set; } = TimeSpan.Zero;

        public Dictionary<string, Type> TypesLibrary = new Dictionary<string, Type>();

        public Dictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>();

        public List<CompiledStatement> CompiledStatements { get; set; } = new List<CompiledStatement>();
    }

    public class State
    {
        public State() { }
        public State(Variable variable) { Variable = variable; }

        public Dictionary<string, object> States { get; set; } = new Dictionary<string, object>();
        public Variable Variable { get; set; }
    }

    public class Variable
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class CompiledStatement
    {
        public TimeSpan TriggerAt { get; set; }
    }

    public class SceneNodeStatement : CompiledStatement
    {
        public string NodeName { get; set; }
    }

    public class SceneCreateStatement : SceneNodeStatement
    {
        public Type Type { get; set; }
        public string[] Parameters { get; set; }
    }

    public class SceneSetPropertyStatement : SceneNodeStatement
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

    public class SceneSetTransitionStatement : SceneNodeStatement
    {
        public Dictionary<string, string> TransitionProperties { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class SceneClassStatement : CompiledStatement
    {
        public string ClassName { get; set; }
    }
}
