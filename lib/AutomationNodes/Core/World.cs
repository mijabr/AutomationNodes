using AutomationNodes.Nodes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public class World : Div
    {
        public virtual Task OnMessage(string message) => Task.CompletedTask;
    }

    public class Worlds : Dictionary<string, World>
    {
    }
}
