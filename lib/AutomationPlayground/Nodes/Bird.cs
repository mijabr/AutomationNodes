using AutomationNodes.Core;
using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;

namespace AutomationPlayground.Nodes
{
    public class Bird : Div, INode
    {
        private readonly INodeCommander nodeCommander;
        private readonly ITemporalEventQueue temporalEventQueue;
        private readonly IWorldTime worldTime;

        public Bird(
            INodeCommander nodeCommander,
            ITemporalEventQueue temporalEventQueue,
            IWorldTime worldTime)
        {
            this.nodeCommander = nodeCommander;
            this.temporalEventQueue = temporalEventQueue;
            this.worldTime = worldTime;
        }

        public override void OnCreated(Clients clients, object[] parameters)
        {
            base.OnCreated(clients, parameters);
            nodeCommander.SetProperty(this, "position", "absolute");
            body = nodeCommander.CreateChildNode<Image>(this, "assets/flying-bird-body.png");
            leftWing = nodeCommander.CreateChildNode<Image>(this, "assets/flying-bird-left-wing.png");
            rightWing = nodeCommander.CreateChildNode<Image>(this, "assets/flying-bird-right-wing.png");
            nodeCommander.SetProperty(body, "z-index", "1");
            SetPropertyForAll("position", "absolute");
            SetSize(parameters[0] as string, parameters[1] as string);
            Flap(TimeSpan.FromSeconds(1));
        }

        public void Flap(TimeSpan duration)
        {
            FlapDown(duration);
            temporalEventQueue.AddFutureEvent(new TemporalEvent {
                TriggerAt = worldTime.Time.Elapsed + duration,
                Action = () => FlapUp(duration)
            });
            temporalEventQueue.AddFutureEvent(new TemporalEvent {
                TriggerAt = worldTime.Time.Elapsed + duration + duration,
                Action = () => FlapDown(duration)
            });
        }

        public void FlapDown(TimeSpan duration)
        {
            nodeCommander.SetTransition(leftWing, new Dictionary<string, string> {
                ["transform"] = "rotate(-80deg)"
            }, duration);
            nodeCommander.SetTransition(rightWing, new Dictionary<string, string> {
                ["transform"] = "rotate(80deg)"
            }, duration);
        }

        public void FlapUp(TimeSpan duration)
        {
            nodeCommander.SetTransition(leftWing, new Dictionary<string, string> {
                ["transform"] = "rotate(0deg)"
            }, duration);
            nodeCommander.SetTransition(rightWing, new Dictionary<string, string> {
                ["transform"] = "rotate(0deg)"
            }, duration);
        }

        private void SetPropertyForAll(string name, string value)
        {
            nodeCommander.SetProperty(body, name, value);
            nodeCommander.SetProperty(leftWing, name, value);
            nodeCommander.SetProperty(rightWing, name, value);

        }

        private void SetSize(string width, string height)
        {
            SetPropertyForAll("width", width);
            SetPropertyForAll("height", height);
        }

        private Image body;
        private Image leftWing;
        private Image rightWing;
    }
}
