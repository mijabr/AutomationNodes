namespace AutomationNodes.Core
{
    public class Caps
    {
        public int windowInnerWidth { get; set; }
        public int windowInnerHeight { get; set; }
        public string userAgent { get; set; }
        public bool isMobile =>
            userAgent.Contains("Android") ||
            userAgent.Contains("webOS") ||
            userAgent.Contains("iPhone") ||
            userAgent.Contains("iPad") ||
            userAgent.Contains("iPod") ||
            userAgent.Contains("BlackBerry") ||
            userAgent.Contains("IEMobile") ||
            userAgent.Contains("Opera Mini");
    }
}
