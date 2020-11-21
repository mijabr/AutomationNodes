using AutomationNodes.Core;
using System.Collections.Generic;

namespace AutomationNodes.Nodes
{
    public class Text : Div
    {
        public string InnerHtml { get; set; }

        public override void OnCreate(object[] parameters)
        {
            base.OnCreate(parameters);

            if (parameters.Length > 0)
            {
                InnerHtml = (string)parameters[0];
            }
        }

        public override Dictionary<string, object> CreationMessage()
        {
            var message = base.CreationMessage();
            message.Add("innerHtml", InnerHtml);
            return message;
        }
    }
}
