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

        private const string script = @"
            using AutomationPlayground;

            Image(assets/two-trees.jpg).set([height:100%]);

            class Bird(width,height) {
                var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
                var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%).transition([transform:rotate(-80deg),duration:333]);
                var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%).transition([transform:rotate(80deg),duration:333]);
                function flap() {
                    leftWing.transition([transform:rotate(0deg),duration:333]).transition([transform:rotate(-80deg),duration:333]);
                    rightWing.transition([transform:rotate(0deg),duration:333]).transition([transform:rotate(80deg),duration:333]);
                };
                function flapfast() {
                    leftWing.transition([transform:rotate(0deg),duration:111]).transition([transform:rotate(-80deg),duration:111]);
                    rightWing.transition([transform:rotate(0deg),duration:111]).transition([transform:rotate(80deg),duration:111]);
                };
                function openWings() {
                    leftWing.transition([transform:rotate(0deg),duration:333]);
                    rightWing.transition([transform:rotate(0deg),duration:333]);
                };
            };

            var myBird1 = Bird(200px,100px).set([left:40%,top:33%]);
            var myBird2 = Bird(200px,100px).set([left:45%,top:4%]);
            var myBird3 = Bird(200px,100px).set([left:60%,top:12%]);
            var myBird4 = Bird(200px,100px).set([left:66%,top:32%]);
            var myBird5 = Bird(200px,100px).set([left:20%,top:53%]);

            myBird1.flap();
            myBird2.flap();
            myBird3.flap();
            myBird4.flap();
            myBird5.flap();

            myBird1.flap();
            myBird2.flap();
            myBird3.flap();
            myBird4.flap();
            myBird5.flap();

            myBird1.flap();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();

            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();

            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();

            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();

            myBird2.wait(2000).transition([left:50%,top:-20%,duration:1000]);
            myBird3.wait(2000).transition([left:70%,top:-20%,duration:1000]);
            myBird4.wait(2000).transition([left:110%,top:32%,duration:1000]);
            myBird5.wait(2000).transition([left:-30%,top:43%,duration:1000]);

            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.flapfast();
            myBird1.wait(2000).transition([left:40%,top:50%,duration:1000]).transition([left:70%,top:92%,transform:rotate(180deg),duration:1000]);
            myBird1.openWings();
            ";
    }
}
