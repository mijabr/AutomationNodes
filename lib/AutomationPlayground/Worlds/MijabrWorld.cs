﻿using AutomationNodes.Core;
using AutomationPlayground.Scenes;

namespace AutomationPlayground.Worlds
{
    public class MijabrWorld : WorldBase
    {
        public MijabrWorld(WorldCatalogue worldCatalogue, WorldTime worldTime, string connectionId, IHubManager hubManager) : base(worldCatalogue, worldTime, connectionId, hubManager)
        {
        }

        public override void OnCreated()
        {
            base.OnCreated();

            SetProperty("width", "1000px");
            SetProperty("height", "1000px");
            SetProperty("color", "white");
            SetProperty("background-color", "black");
            SetProperty("overflow", "hidden");

            new MijabrScene(this).Run();
            new ShipScene(this).Run();
            new RandomShipScene(this).Run();
        }
    }
}
