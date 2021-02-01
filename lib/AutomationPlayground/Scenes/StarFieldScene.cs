using AutomationNodes.Core;
using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AutomationPlayground.Scenes
{
    public class StarFieldScene : IScene
    {
        private readonly INodeCommander nodeCommander;

        public StarFieldScene(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
        }

        public void Run(string connectionId)
        {
            var state = new State(connectionId);
            Task.Run(() => RunAsync(state));
        }

        private TimeSpan starLifetime = TimeSpan.FromSeconds(5);

        private class Star
        {
            public Text Node { get; set; }
            public TimeSpan StartedAt { get; set; }
        }

        private class State
        {
            public State(string ConnectionId)
            {
                this.ConnectionId = ConnectionId;
            }

            public Random Random { get; } = new();
            public string ConnectionId { get; }
            public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
            public Queue<Star> Stars { get; } = new();
            public TimeSpan FlowRate { get; set; } = TimeSpan.FromMilliseconds(30);
            public TimeSpan LastFlowRateChange { get; set; }
            public TimeSpan FlowChangeTimer { get; set; } = TimeSpan.FromSeconds(1);
            public void ChangeFlow()
            {
                FlowRate = TimeSpan.FromMilliseconds(Random.Next(10, 300));
                LastFlowRateChange = Stopwatch.Elapsed;
                FlowChangeTimer = TimeSpan.FromSeconds(1);
            }
        }

        private async Task RunAsync(State state)
        {
            while (true)
            {
                AddStar(state);
                await Task.Delay(state.FlowRate);
                if (state.Stopwatch.Elapsed > state.LastFlowRateChange + state.FlowChangeTimer)
                {
                    state.ChangeFlow();
                }
            }
        }

        private void AddStar(State state)
        {
            var star = GetStar(state);
            var props = new Dictionary<string, string>
            {
                ["transition-timing-function"] = "cubic-bezier(0.9,0,1,1)",
                ["color"] = "white"
            };
            var position = state.Random.NextDouble() * 100.0;
            switch (state.Random.Next(0, 4))
            {
                case 0:
                    props.Add("left", "-10%");
                    props.Add("top", $"{position}%");
                    break;
                case 1:
                    props.Add("left", "110%");
                    props.Add("top", $"{position}%");
                    break;
                case 2:
                    props.Add("left", $"{position}%");
                    props.Add("top", "-10%");
                    break;
                case 3:
                    props.Add("left", $"{position}%");
                    props.Add("top", "110%");
                    break;
            }

            star.StartedAt = state.Stopwatch.Elapsed;
            nodeCommander.SetTransition(star.Node, props, starLifetime);
        }

        private Star GetStar(State state)
        {
            Star star;
            if (state.Stars.Count > 0 && state.Stopwatch.Elapsed > state.Stars.Peek().StartedAt + starLifetime + TimeSpan.FromSeconds(1))
            {
                star = state.Stars.Dequeue();
            }
            else
            {
                star = NewStar(state);
            }
            nodeCommander.SetProperty(star.Node, "transition-timing-function", " ");
            nodeCommander.SetProperty(star.Node, "transition-duration", " ");
            nodeCommander.SetProperty(star.Node, "position", "absolute");
            nodeCommander.SetProperty(star.Node, "color", "#808080");
            nodeCommander.SetProperty(star.Node, "left", "50%");
            nodeCommander.SetProperty(star.Node, "top", "50%");

            state.Stars.Enqueue(star);
            return star;
        }

        private Star NewStar(State state)
        {
            Star star = new Star
            {
                Node = nodeCommander.CreateNode<Text>(state.ConnectionId, ".")
            };
            return star;
        }
    }
}
