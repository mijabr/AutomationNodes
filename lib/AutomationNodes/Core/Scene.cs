namespace AutomationNodes.Core
{
    public class ClientContext
    {
        public string ConnectionId { get; set; }
        public Caps Caps { get; set; }
        public double ImageScaling { get; set; } = 1.0;
        public double FontScaling { get; set; } = 1.0;
        public string ScaledImage(double size) => $"{size * ImageScaling}%";
        public string ScaledFont(double size) => $"{size * FontScaling}em";
    }

    public interface IScene
    {
        void Run(ClientContext clientContext);
    }
}
