using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomationNodes.Core
{
    public interface IHubMessenger
    {
        void SendWorldMessage(string connectionId, Guid worldId);
        void SendCreationMessage(string connectionId, Dictionary<string, object> creationMessage);
        void SendSetPropertyMessage(string connectionId, Guid nodeId, string propertyName, string propertyValue);
        void SendTransitionMessage(string connectionId, Guid nodeId, Dictionary<string, string> transitionProperties, TimeSpan duration, bool destroyAfter);
        void SendAddKeyframeMessage(string connectionId, Dictionary<string, string> keyframeProperties, string keyframeName, string keyframePercent);
    }

    public class HubMessenger : IHubMessenger
    {
        private readonly IHubDownstream hubDownstream;
        private readonly IPropertyScaler propertyScaler;

        public HubMessenger(
            IHubDownstream hubDownstream,
            IPropertyScaler propertyScaler)
        {
            this.hubDownstream = hubDownstream;
            this.propertyScaler = propertyScaler;
        }

        public void SendWorldMessage(string connectionId, Guid worldId)
        {
            hubDownstream.Send(connectionId, new Dictionary<string, object>
            {
                { "message", "World" },
                { "id", worldId }
            });
        }

        public void SendCreationMessage(string connectionId, Dictionary<string, object> creationMessage)
        {
            creationMessage["message"] = "Create";
            hubDownstream.Send(connectionId, creationMessage);
        }

        public void SendSetPropertyMessage(string connectionId, Guid nodeId, string propertyName, string propertyValue)
        {
            if ((propertyName == "width" || propertyName == "height") && propertyValue != "100%")
            {
                propertyValue = propertyScaler.ScaleImageProperty(connectionId, propertyValue);
            }
            else if (propertyName == "font-size")
            {
                propertyValue = propertyScaler.ScaleFontSizeProperty(connectionId, propertyValue);
            }

            hubDownstream.Send(connectionId, new Dictionary<string, object>
            {
                    { "message", "SetProperty" },
                    { "id", nodeId },
                    { "name", propertyName },
                    { "value", propertyValue }
            });
        }

        public void SendTransitionMessage(string connectionId, Guid nodeId, Dictionary<string, string> transitionProperties, TimeSpan duration, bool destroyAfter)
        {
            var scaledTransitionProperties = transitionProperties.Select(property =>
            {
                if ((property.Key == "width" || property.Key == "height") && property.Value != "100%")
                {
                    return new KeyValuePair<string, string>(property.Key, propertyScaler.ScaleImageProperty(connectionId, property.Value));
                }
                else if (property.Key == "font-size")
                {
                    return new KeyValuePair<string, string>(property.Key, propertyScaler.ScaleFontSizeProperty(connectionId, property.Value));
                }

                return property;
            }).ToDictionary(kv => kv.Key, kv => kv.Value);

            hubDownstream.Send(connectionId, new Dictionary<string, object>
            {
                    { "message", "SetTransition" },
                    { "id", nodeId },
                    { "properties", scaledTransitionProperties },
                    { "duration", duration.TotalMilliseconds },
                    { "destroyAfter", destroyAfter }
            });
        }

        public void SendAddKeyframeMessage(string connectionId, Dictionary<string, string> keyframeProperties, string keyframeName, string keyframePercent)
        {
            hubDownstream.Send(connectionId, new Dictionary<string, object>
            {
                { "message", "AddKeyframe" },
                { "properties", keyframeProperties },
                { "keyframename", keyframeName },
                { "keyframepercent", keyframePercent }
            });
        }
    }
}
