# Automate and Animate

Create and animate HTML from C# using a simple DSL

```c#
using AutomationNodes.Core;

namespace AutomationPlayground.Scenes
{
    public class RocketElephantScene : SceneBase
    {
        public RocketElephantScene(IWorld world) : base(world)
        {
        }

        public string Script => @"
            using AutomationPlayground;

            Image(assets/ship-0001.svg)
                .set({position:absolute,left:10%,top:80%,width:100px,height:100px})
                .transition({top:20%,width:300px,height:300px,duration:3000})
                .transition({left:30%,top:10%,transform:rotate(90deg)},duration:1000)
                .transition({top:20%,left:50%,transform:rotate(180deg),duration:1000})
                .transition({top:74%,left:70%,width:100px,height:100px,duration:2000});

            @(4000);
            Image(assets/elephant-sitting.png)
                .set({position:absolute,opacity:0,left:90%,top:83%,width:200px,height:200px})
                .transition({opacity:0.2,left:70%,duration:1000})
                .transition({opacity:1,duration:2000});

            @(7000);
            SpeechBubble(Nice landing!)
                .set({position:absolute,opacity:0,left:60%,top:90%,width:150px})
                .transition({opacity:1,duration:1000})
                .wait(2000)
                .transition({opacity:0,duration:1000});
";

        public void Run()
        {
            Run(Script);
        }
    }
}
```
