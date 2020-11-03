using AutomationNodes.Core;

namespace AutomationPlayground.Scenes
{
    public class MijabrScene : SceneBase
    {
        public MijabrScene(WorldBase world) : base(world)
        {
        }

        public string Script => @"
            TextNode(M).set({position:absolute,left:100px,top:100px,color:black}).transition({left:0px,top:0px,color:white,duration:1000});
            TextNode(I).set({position:absolute,left:200px,top:300px,color:black}).transition({left:15px,top:0px,color:white,duration:1000});
            TextNode(J).set({position:absolute,left:300px,top:600px,color:black}).transition({left:22px,top:0px,color:white,duration:1000});
            TextNode(A).set({position:absolute,left:400px,top:1000p,color:blackx}).transition({left:28px,top:0px,color:white,duration:1000});
            TextNode(B).set({position:absolute,left:500px,top:800px,color:black}).transition({left:40px,top:0px,color:white,duration:1000});
            TextNode(R).set({position:absolute,left:600px,top:200px,color:black}).transition({left:52px,top:0px,color:white,duration:1000});";

        public void Run()
        {
            Run(Script);
        }
    }
}
