using System.Threading.Tasks;

namespace AutomationNodes.Core
{
    public interface IAutomationHubContext
    {
        Task Send(string connectionId, AutomationBase node);
    }
}
