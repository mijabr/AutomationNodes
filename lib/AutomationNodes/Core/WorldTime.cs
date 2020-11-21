using System.Diagnostics;

namespace AutomationNodes.Core
{
    public interface IWorldTime
    {
        Stopwatch Time { get; }
    }

    public class WorldTime : IWorldTime
    {
        public Stopwatch Time { get; } = Stopwatch.StartNew();
    }
}
