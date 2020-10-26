using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public interface IAutomationHubContext
    {
        Task Send(string connectionId, List<AutomationBase> nodes);
    }

    public interface IHubManager
    {
        void Send(string connectionId, AutomationBase node);
    }
}
