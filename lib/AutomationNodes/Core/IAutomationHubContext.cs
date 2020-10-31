using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public interface IAutomationHubContext
    {
        Task Send(string connectionId, List<AutomationMessage> messages);
    }

    public interface IHubManager
    {
        void Send(string connectionId, AutomationMessage message);
    }
}
