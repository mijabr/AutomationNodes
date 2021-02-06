using AutomationNodes.Core;

namespace AutomationPlayground.Scenes
{
    public class MijabrScene : IScene
    {
        private readonly ISceneActioner sceneActioner;
        private readonly IConnectedClients connectedClients;

        public MijabrScene(
            ISceneActioner sceneActioner,
            IConnectedClients connectedClients)
        {
            this.sceneActioner = sceneActioner;
            this.connectedClients = connectedClients;
        }

        public void Run(Clients clients)
        {
            var scaling = 1.0;
            if (!clients.IsAll)
            {
                if (connectedClients.ClientContexts.TryGetValue(clients.ConnectionIds[0], out var clientContext))
                {
                    scaling = clientContext.FontScaling * 1.3;
                }
            }
            
            var script = @$"
            Text(M).set([position:absolute,left:50%,top:50%,color:black,font-size:1em).transition([left:0px,top:0px,color:white,duration:1000]);
            @(100);Text(I).set([position:absolute,left:50%,top:50%,color:black,font-size:1em]).transition([left:{scaling * 15}px,top:0px,color:white,duration:900]);
            @(200);Text(J).set([position:absolute,left:50%,top:50%,color:black,font-size:1em]).transition([left:{scaling * 22}px,top:0px,color:white,duration:800]);
            @(300);Text(A).set([position:absolute,left:50%,top:50%,color:black,font-size:1em]).transition([left:{scaling * 31}px,top:0px,color:white,duration:700]);
            @(400);Text(B).set([position:absolute,left:50%,top:50%,color:black,font-size:1em]).transition([left:{scaling * 43}px,top:0px,color:white,duration:600]);
            @(500);Text(R).set([position:absolute,left:50%,top:50%,color:black,font-size:1em]).transition([left:{scaling * 55}px,top:0px,color:white,duration:500]);";

            sceneActioner.Run(clients, script);
        }
    }
}
