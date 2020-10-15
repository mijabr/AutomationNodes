using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AutomationNodes.Core
{
    public class WorldTime
    {
        public Stopwatch Time { get; set; } = Stopwatch.StartNew();
    }
}
