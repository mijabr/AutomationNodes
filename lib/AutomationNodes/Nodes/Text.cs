﻿using AutomationNodes.Core;
using System.Collections.Generic;

namespace AutomationNodes.Nodes
{
    public class Text : Div
    {
        public Text(WorldBase world, string text) : base(world)
        {
            InnerHtml = text;
        }
        public string InnerHtml { get; set; }

        public override Dictionary<string, object> CreationMessage()
        {
            var message = base.CreationMessage();
            message.Add("innerHtml", InnerHtml);
            return message;
        }
    }
}