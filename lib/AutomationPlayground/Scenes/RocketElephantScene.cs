using AutomationNodes.Core;

namespace AutomationPlayground.Scenes
{
    public class RocketElephantScene : IScene
    {
        private readonly ISceneActioner sceneActioner;

        public RocketElephantScene(ISceneActioner sceneActioner)
        {
            this.sceneActioner = sceneActioner;
        }

        public void Run(ClientContext clientContext)
        {
            sceneActioner.Run(script, clientContext.ConnectionId);
        }

        private const string script = @"
            using AutomationPlayground;

            Image(assets/ship-0001.svg,100px,100px)
                .set([left:10%,top:80%])
                .transition([top:20%,width:300px,height:300px,duration:3000])
                .transition([left:30%,top:10%,transform:rotate(90deg)],duration:1000)
                .transition([top:20%,left:50%,transform:rotate(180deg),duration:1000])
                .transition([top:74%,left:70%,width:100px,height:100px,duration:2000]);

            @(4000);
            Image(assets/elephant-sitting.png,200px,200px)
                .set([opacity:0,left:90%,top:83%])
                .transition([opacity:0.2,left:70%,duration:1000])
                .transition([opacity:1,duration:2000]);

            @(7000);
            SpeechBubble(Nice landing!)
                .set([opacity:0,left:60%,top:90%,width:150px])
                .transition([opacity:1,duration:1000])
                .wait(2000)
                .transition([opacity:0,duration:1000]);";
    }
}
