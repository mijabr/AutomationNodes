using System.Threading.Tasks;

namespace AutomationNodes
{
    public interface IAutomationHubContext
    {
        Task Send(string connectionId, AutomationBase node);
    }
}
