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

        public const string bird = @"
            Bird(width, height) : Div {
                body = Image(assets/flying-bird-body.png,%width%,%height%).set({z-index:1});
                leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%);
                rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%);
                flap(duration) {
                    leftWing.transition([transform:rotate(-80deg)]);
                    rightWing.transition([transform:rotate(80deg)]);
                    @%duration%
                    leftWing.transition([transform:rotate(0deg)]);
                    rightWing.transition([transform:rotate(0deg)]);
                }
            };
        ";

        private const string script = @"
            using AutomationPlayground;

            Image(assets/two-trees.jpg).set([height:100%]);

            bird = Bird(200px,100px).set([left:40%,top:33%]);

            //bird.flap(500);
";
    }
}
