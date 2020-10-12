using System.Threading;

namespace AutomationApp
{
    public class ApplicationRunningToken
    {
        public CancellationTokenSource CancellationToken { get; set; } = new CancellationTokenSource();
    }
}
