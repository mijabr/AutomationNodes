using System;
using System.Collections.Generic;

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

        public HubMessenger(IHubDownstream hubDownstream)
        {
            this.hubDownstream = hubDownstream;
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
            hubDownstream.Send(connectionId, new Dictionary<string, object>
            {
                    { "message", "SetTransition" },
                    { "id", nodeId },
                    { "properties", transitionProperties },
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
