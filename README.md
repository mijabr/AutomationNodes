# Automate and Animate

### Quick example
Create, move and animate with C#

```c#
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

        public void Run(string connectionId)
        {
            sceneActioner.Run(script, connectionId);
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

```

### Tutorial

##### Create a DOM element

```
Div();
```

##### Create an Image element

```
Image(assets/rocket.png);
Image(assets/rocket.png,200px,200px);
```

##### Create an Image element and set its properties

```
Image(assets/rocket.png).set([left:100px,top:50%]);
Image(assets/rocket.png).set([transform:rotate(40.4.deg)]);
```
You can set any properties available on the HTML DOM element you created. The set command takes an array of property setters. By default, Image nodes are set to position:absolute.

##### Perform transitions

```
Image(assets/rocket.png).set([left:20%,top:20%]).transition([left:50%,top:50%,duration:1000]);
```
The transition command takes an array of transition properties, one of which must be 'duration'. Duration is in milliseconds.

##### Wait command

```
Image(assets/rocket.png).set([left:20%,top:20%]).wait(1000).transition([left:50%,top:50%,duration:1000]);
```
Waits are in milliseconds.

##### Global Wait command

```
@(4000)
Image(assets/rocket.png).set([left:20%,top:20%]).transition([left:50%,top:50%,duration:1000]);
```
Waits are in milliseconds.

##### Define variables

```
var ship = Image(assets/rocket.png).set([left:20%,top:20%]);
ship.transition([left:50%,top:50%,duration:1000]);
```
Script syntax follows the Builder pattern so you can chain commands and do an assignment to a variable at the same time.

##### Define Functions

```
var ship = Image(assets/rocket.png).set([left:0px,top:0px]);
function flyTo(x,y) {
    ship.transition([left:%x%,top:%y%,duration:1000]);
};
ship.flyTo(100px,200px);
ship.flyTo(200px,300px);
```

##### Define Classes

```
class Bird(width,height) {
    var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
    var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%);
    var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%);
};
var myBird = Bird(100px,200px).set([left:500px,top:300px]).transition([left:200px,duration:1000]);
```
Classes are nested DOM elements. The parent is a DIV element. Child elements are positioned relative to their parent class element. Once created, set and transition commands performed on the class are applied to the parent element.

##### Define Class Functions

```
class Bird(width,height) {
    var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
    var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%);
    var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%);
    function flap() {
        leftWing.transition([transform:rotate(-80deg),duration:300]).transition([transform:rotate(0deg),duration:300]);
        rightWing.transition([transform:rotate(80deg),duration:300]).transition([transform:rotate(0deg),duration:300]);
    };
};
var myBird = Bird(100px,200px).set([left:500px,top:300px]);
myBird.flap();
myBird.flap();
myBird.flap();
```

##### Define custom nodes in C#
Optionally, you can create custom elements directly in C# by extending an INode class.

```c#
using AutomationNodes.Core;
using AutomationNodes.Nodes;

namespace AutomationPlayground.Nodes
{
    public class SpeechBubble : Div
    {
        private readonly INodeCommander nodeCommander;

        public SpeechBubble(INodeCommander nodeCommander)
        {
            this.nodeCommander = nodeCommander;
        }

        private string Text { get; set; }

        public override void OnCreated(object[] parameters)
        {
            base.OnCreated(parameters);

            if (parameters.Length > 0)
            {
                Text = (string)parameters[0];
            }

            nodeCommander.SetProperty(this, "position", "absolute");

            var bubble = nodeCommander.CreateChildNode<Text>(this, Text);
            nodeCommander.SetProperty(bubble, "background", "white");
            nodeCommander.SetProperty(bubble, "color", "black");
            nodeCommander.SetProperty(bubble, "border-radius", "4em");
            nodeCommander.SetProperty(bubble, "position", "absolute");
            nodeCommander.SetProperty(bubble, "padding", "10px");
        }
    }
}
```

