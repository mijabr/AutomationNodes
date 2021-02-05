using System.Collections.Generic;

namespace AutomationNodes.Core
{
    public class KeyFrame
    {
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public string Percent { get; set; }
    }
}
