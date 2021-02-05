using AutomationNodes.Nodes;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public class World : Div
    {
        public virtual Task OnMessage(string connectionId, string message) => Task.CompletedTask;
    }
}
