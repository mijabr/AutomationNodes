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

            keyframe([keyframe-name:flap-left-wing,keyframe-percent:0%,transform:rotate(-80deg)]);
            keyframe([keyframe-name:flap-left-wing,keyframe-percent:100%,transform:rotate(0deg)]);
            keyframe([keyframe-name:flap-right-wing,keyframe-percent:0%,transform:rotate(80deg)]);
            keyframe([keyframe-name:flap-right-wing,keyframe-percent:100%,transform:rotate(0deg)]);
            keyframe([keyframe-name:flap-right-wing-wonkily,keyframe-percent:100%,transform:rotate(80deg)]);
            keyframe([keyframe-name:flap-right-wing-wonkily,keyframe-percent:0%,transform:rotate(0deg)]);

            class Bird(width,height) {
                var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
                var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%).set([transform:rotate(-80deg),animation-iteration-count:infinite,animation-direction:alternate,animation-timing-function:linear]);
                var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%).set([transform:rotate(80deg),animation-iteration-count:infinite,animation-direction:alternate,animation-timing-function:linear]);
                function flap() {
                    leftWing.set([animation-duration:0.3s,animation-name:flap-left-wing]);
                    rightWing.set([animation-duration:0.3s,animation-name:flap-right-wing]);
                };
                function flapfast() {
                    leftWing.set([animation-duration:0.1s,animation-name:flap-left-wing]);
                    rightWing.set([animation-duration:0.1s,animation-name:flap-right-wing]);
                };
                function flapwonkily() {
                    leftWing.set([animation-duration:0.22s,animation-name:flap-left-wing]);
                    rightWing.set([animation-duration:0.16s,animation-name:flap-right-wing-wonkily]);
                };
                function stopflapping() {
                    leftWing.set([animation-name: ]);
                    rightWing.set([animation-name: ]);
                };
                function openWings() {
                    leftWing.transition([transform:rotate(0deg),duration:700]);
                    rightWing.transition([transform:rotate(0deg),duration:700]);
                };
            };

            keyframe([keyframe-name:flap-butterfly-wing,keyframe-percent:0%,transform:ScaleY(1) scaleX(-1)]);
            keyframe([keyframe-name:flap-butterfly-wing,keyframe-percent:100%,transform:ScaleY(0.1) scaleX(-1)]);

            class Butterfly(width,height) {
                var bbody = Image(assets/butterfly-body.png,%width%,%height%).set([z-index:1]);
                var wing = Image(assets/butterfly-wing.png,%width%,%height%).set([animation-iteration-count:infinite,animation-direction:alternate]);
                function turnToLeft() {
                    bbody.transition([transform:scaleX(-1)],duration:500);
                    wing.transition([transform:scaleX(-1)],duration:500);
                };
                function turnToRight() {
                    bbody.transition([transform:scaleX(1)],duration:500);
                    wing.transition([transform:scaleX(1)],duration:500);
                };
                function flapWings() {
                    wing.set([animation-duration:0.75s,animation-name:flap-butterfly-wing]);
                };
                function stopflappingWings() {
                    wing.set([animation-duration:0.75s,animation-name: ]);
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

            @(2000);
            myBird1.flapwonkily();
            myBird2.flapfast();
            myBird3.flapfast();
            myBird4.flapfast();
            myBird5.flapfast();

            @(4000);
            myBird1.transition([left:40%,top:50%,duration:1000]).transition([left:70%,top:92%,transform:rotate(180deg),duration:1000]);
            myBird2.transition([left:50%,top:-20%,duration:1000]);
            myBird3.transition([left:70%,top:-20%,duration:1000]);
            myBird4.transition([left:110%,top:32%,duration:1000]);
            myBird5.transition([left:-30%,top:43%,duration:1000]);

            @(6000);
            myBird1.stopflapping();
            myBird1.openWings();

            @(7000)
            var butterfly = Butterfly(100px,100px).set([left:100%,top:20%]);
            butterfly.turnToLeft();
            butterfly.transition([left:30%,top:31%,duration:5000]);
            butterfly.flapWings();

            @(12000)
            butterfly.stopflappingWings();
            butterfly.turnToRight();

            @(13000)
            SpeechBubble(Nice try!)
                .set([opacity:0,left:37%,top:32%,width:150px])
                .transition([opacity:1,duration:1000])
                .wait(2000)
                .transition([opacity:0,duration:1000]);
            ";
    }
}
