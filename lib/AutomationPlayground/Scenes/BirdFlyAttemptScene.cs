using AutomationNodes.Core;

namespace AutomationPlayground.Scenes
{
    public class BirdFlyAttemptScene : IScene
    {
        private readonly ISceneActioner sceneActioner;

        public BirdFlyAttemptScene(ISceneActioner sceneActioner)
        {
            this.sceneActioner = sceneActioner;
        }

        public void Run(string connectionId)
        {
            sceneActioner.Run(script, connectionId);
        }
            //:Bird(width, height) = Div[
            //    body = Image(assets / flying - bird - body.png).set({position:absolute,z-index:1,width:%width%,height:%height%});
            //    leftWing = Image(assets/flying-bird-left-wing.png).set({ position: absolute,width:% width %,height:% height %});
            //    rightWing = Image(assets/flying-bird-right-wing.png).set({ position: absolute,width:% width %,height:% height %});
            //    Flap(duration) = 
            //];

        private const string script = @"
            using AutomationPlayground;

            Image(assets/two-trees.jpg).set([height:100%]);

            bird = Bird(200px,100px).set([left:40%,top:33%]);

            //bird.Flap(500);
";
    }
}
