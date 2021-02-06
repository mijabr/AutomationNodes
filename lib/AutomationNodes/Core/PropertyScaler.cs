namespace AutomationNodes.Core
{
    public interface IPropertyScaler
    {
        string ScaleImageProperty(string connectionId, string propertyValue);
        string ScaleFontSizeProperty(string connectionId, string propertyValue);
    }

    public class PropertyScaler : IPropertyScaler
    {
        private readonly IConnectedClients connectedClients;

        public PropertyScaler(IConnectedClients connectedClients)
        {
            this.connectedClients = connectedClients;
        }

        public string ScaleImageProperty(string connectionId, string propertyValue)
        {
            if (!connectedClients.ClientContexts.TryGetValue(connectionId, out var clientContext)
            || clientContext.ImageScaling == 1.0)
            {
                return propertyValue;
            }

            propertyValue = propertyValue.Trim();

            var lastDigitIndex = propertyValue.LastIndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' });
            if (double.TryParse(propertyValue.Substring(0, lastDigitIndex + 1), out var number))
            {
                number *= clientContext.ImageScaling;
                return $"{number}{propertyValue.Substring(lastDigitIndex + 1)}";
            }


            return propertyValue;
        }

        public string ScaleFontSizeProperty(string connectionId, string propertyValue)
        {
            if (!connectedClients.ClientContexts.TryGetValue(connectionId, out var clientContext)
            || clientContext.ImageScaling == 1.0)
            {
                return propertyValue;
            }

            propertyValue = propertyValue.Trim();

            var lastDigitIndex = propertyValue.LastIndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' });
            if (double.TryParse(propertyValue.Substring(0, lastDigitIndex + 1), out var number))
            {
                number *= clientContext.FontScaling;
                return $"{number}{propertyValue.Substring(lastDigitIndex + 1)}";
            }


            return propertyValue;
        }
    }
}
