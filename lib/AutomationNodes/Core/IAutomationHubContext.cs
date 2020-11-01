using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public interface IAutomationHubContext
    {
        Task Send(string connectionId, List<Dictionary<string, object>> messages);
    }

    public interface IHubManager
    {
        void Send(string connectionId, Dictionary<string, object> message);
    }
}
