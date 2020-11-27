﻿using AutomationNodes.Core;

namespace AutomationPlayground.Scenes
{
    public class MijabrScene : IScene
    {
        private readonly ISceneActioner sceneActioner;

        public MijabrScene(ISceneActioner sceneActioner)
        {
            this.sceneActioner = sceneActioner;
        }

        public void Run(string connectionId)
        {
            sceneActioner.Run(script, connectionId);
        }

        private const string script = @"
            Text(M).set([position:absolute,left:100px,top:100px,color:black]).transition([left:0px,top:0px,color:white,duration:1000]);
            Text(I).set([position:absolute,left:200px,top:300px,color:black]).transition([left:15px,top:0px,color:white,duration:1000]);
            Text(J).set([position:absolute,left:300px,top:600px,color:black]).transition([left:22px,top:0px,color:white,duration:1000]);
            Text(A).set([position:absolute,left:400px,top:1000p,color:black]).transition([left:28px,top:0px,color:white,duration:1000]);
            Text(B).set([position:absolute,left:500px,top:800px,color:black]).transition([left:40px,top:0px,color:white,duration:1000]);
            Text(R).set([position:absolute,left:600px,top:200px,color:black]).transition([left:52px,top:0px,color:white,duration:1000]);";
    }
}
