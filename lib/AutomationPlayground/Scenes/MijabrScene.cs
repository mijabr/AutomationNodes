using AutomationNodes.Core;

namespace AutomationPlayground.Scenes
{
    public class MijabrScene : IScene
    {
        private readonly ISceneActioner sceneActioner;

        public MijabrScene(ISceneActioner sceneActioner)
        {
            this.sceneActioner = sceneActioner;
        }

        public void Run(ClientContext clientContext)
        {
            var script = @$"
            Text(M).set([position:absolute,left:50%,top:50%,color:black]).transition([left:0px,top:0px,color:white,duration:1000]);
            @(100);Text(I).set([position:absolute,left:50%,top:50%,color:black]).transition([left:{clientContext.FontScaling * 15}px,top:0px,color:white,duration:900]);
            @(200);Text(J).set([position:absolute,left:50%,top:50%,color:black]).transition([left:{clientContext.FontScaling * 22}px,top:0px,color:white,duration:800]);
            @(300);Text(A).set([position:absolute,left:50%,top:50%,color:black]).transition([left:{clientContext.FontScaling * 29}px,top:0px,color:white,duration:700]);
            @(400);Text(B).set([position:absolute,left:50%,top:50%,color:black]).transition([left:{clientContext.FontScaling * 41}px,top:0px,color:white,duration:600]);
            @(500);Text(R).set([position:absolute,left:50%,top:50%,color:black]).transition([left:{clientContext.FontScaling * 53}px,top:0px,color:white,duration:500]);";

            sceneActioner.Run(script, clientContext.ConnectionId);
        }
    }
}
