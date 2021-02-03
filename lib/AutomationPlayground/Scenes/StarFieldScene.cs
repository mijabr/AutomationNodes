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

        public void Run(ClientContext clientContext)
        {
            var state = new State(clientContext);
            Task.Run(async () => await RunAsync(state));
        }

        private class Star
        {
            public INode Node { get; set; }
            public TimeSpan StartedAt { get; set; }
            public TimeSpan Lifetime { get; set; }
            public double Size { get; set; }
        }

        private class State
        {
            public State(ClientContext ClientContext)
            {
                this.ClientContext = ClientContext;
            }

            public ClientContext ClientContext { get; set; }
            public Random Random { get; } = new();
            public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
            public SortedList<TimeSpan, Star> Stars { get; } = new(new DuplicateKeyComparer<TimeSpan>());
            public TimeSpan FlowRate { get; set; } = TimeSpan.FromMilliseconds(15);
            public TimeSpan LastFlowRateChange { get; set; }
            public TimeSpan FlowChangeTimer { get; set; } = TimeSpan.FromMilliseconds(1800);
            public void ChangeFlow()
            {
                FlowRate = TimeSpan.FromMilliseconds(Random.Next(6, 200));
                LastFlowRateChange = Stopwatch.Elapsed;
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

            nodeCommander.SetProperties(star.Node, new Dictionary<string, string>
            {
                ["transition-timing-function"] = " ",
                ["transition-duration"] = " ",
                ["position"] = "absolute",
                ["left"] = "50%",
                ["top"] = "50%",
                ["width"] = state.ClientContext.ScaledImage(star.Size),
                ["height"] = state.ClientContext.ScaledImage(star.Size)
            });

            var props = new Dictionary<string, string>
            {
                ["transition-timing-function"] = "cubic-bezier(0.9,0,1,1)",
                ["width"] = state.ClientContext.ScaledImage(star.Size * 2),
                ["height"] = state.ClientContext.ScaledImage(star.Size * 2)
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

            nodeCommander.SetTransition(star.Node, props, star.Lifetime);
        }

        private Star GetStar(State state)
        {
            var star = GetOrCreateStar(state);
            star.StartedAt = state.Stopwatch.Elapsed;
            star.Lifetime = TimeSpan.FromMilliseconds(state.Random.Next(5000, 50000));
            state.Stars.Add(star.StartedAt + star.Lifetime, star);
            star.Size = state.Random.NextDouble() * 0.35;
            return star;
        }

        private Star GetOrCreateStar(State state)
        {
            if (state.Stars.Count > 0)
            {
                var s = state.Stars.Values[0];
                if (state.Stopwatch.Elapsed > s.StartedAt + s.Lifetime)
                {
                    state.Stars.RemoveAt(0);
                    return s;
                }
            }

            return new Star
            {
                Node = nodeCommander.CreateNode<Image>(state.ClientContext.ConnectionId, RandomStarColor(state))
            };
        }

        private string RandomStarColor(State state)
        {
            switch (state.Random.Next(0, 100))
            {
                case 0: return "assets/star-red.svg";
                case 1: return "assets/star-green.svg";
                case 2: return "assets/star-blue.svg";
                default: return "assets/star.svg";
            }
        }
    }

    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);
            return result == 0 ? 1 : result;
        }
    }
}
