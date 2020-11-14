using AutomationNodes.Core;
using AutomationNodes.Nodes;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AutomationNodesTests
{
    public class SceneBaseTests
    {
        public class MyNode : AutomationBase
        {
            public MyNode(IWorld world, string param1) : base(world)
            {
            }

            public override string Type => "MyType";
        }

        private class TestState
        {
            public SceneBase Scene { get; }
            public Mock<IWorld> World { get; }
            public Mock<INode> Div1 { get; }
            public Mock<INode> Div2 { get; }
            public Mock<INode> Div3 { get; }
            public Mock<INode> Image1 { get; }
            public Mock<INode> Image2 { get; }
            public Mock<INode> MyNode { get; }

            public TestState()
            {
                Div1 = new Mock<INode>();
                Div2 = new Mock<INode>();
                Div3 = new Mock<INode>();
                Image1 = new Mock<INode>();
                Image2 = new Mock<INode>();
                MyNode = new Mock<INode>();
                World = new Mock<IWorld>();
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(Div)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "1")))).Returns(Div1.Object);
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(Div)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "2")))).Returns(Div2.Object);
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(Div)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "3")))).Returns(Div3.Object);
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(Image)),
                    It.IsAny<object[]>())).Returns(Image1.Object);
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(Image)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "assets/ship-0001.svg")))).Returns(Image1.Object);
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(Image)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "assets/elephant-sitting.png")))).Returns(Image2.Object);
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(MyNode)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "my parameter")))).Returns(MyNode.Object);

                World.Setup(w => w.AddFutureEvent(It.IsAny<TemporalEvent>())).Callback<TemporalEvent>(t => t.Action?.Invoke());

                Scene = new SceneBase(World.Object);
            }
        }

        [Test]
        public void Run_ShouldCreateTextNode_GivenDeclaration()
        {
            var state = new TestState();

            state.Scene.Run("Div(1)");

            state.World.Verify(w => w.CreateNode(
                It.Is<Type>(t => t == typeof(Div)),
                It.Is<object[]>(p => string.Equals((string)p[0], "1"))), Times.Once);
        }

        [Test]
        public void Run_ShouldSetProperties_GivenSetCommand()
        {
            var state = new TestState();

            state.Scene.Run("Div(1).set({position:absolute,left:100px,top:100px})");

            state.Div1.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "position")),
                It.Is<string>(v => string.Equals(v, "absolute"))), Times.Once);
            state.Div1.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "left")),
                It.Is<string>(v => string.Equals(v, "100px"))), Times.Once);
            state.Div1.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "top")),
                It.Is<string>(v => string.Equals(v, "100px"))), Times.Once);
        }

        [Test]
        public void Run_ShouldSetTransition_GivenTransitionCommand()
        {
            var state = new TestState();

            state.Scene.Run("Div(1).set({position:absolute,left:100px,top:100px}).transition({left:0px,top:0px,duration:1000})");

            state.Div1.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "0px") && string.Equals(p["top"], "0px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))
            ), Times.Once);
        }

        [Test]
        public void Run_ShouldSetTransition_GivenTransitionCommandWithWhiteSpace()
        {
            var state = new TestState();

            state.Scene.Run(@"
                Div(1).set({position:absolute,left:100px,top:100px}).transition({left:0px,top:0px,duration:1000})");

            state.Div1.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "0px") && string.Equals(p["top"], "0px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))
            ), Times.Once);
        }

        [Test]
        public void Run_ShouldProcessMultipleStatements_GivenMultiLineScript()
        {
            var state = new TestState();

            state.Scene.Run(@"
                Div(1).set({position:absolute,left:100px,top:100px}).transition({left:0px,top:0px,duration:1000});
                Div(2).set({position:absolute,left:200px,top:200px}).transition({left:10px,top:10px,duration:1000});
                Div(3).set({position:absolute,left:300px,top:300px}).transition({left:20px,top:20px,duration:1000});
");

            state.Div1.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "0px") && string.Equals(p["top"], "0px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))
            ), Times.Once);
            state.Div2.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "10px") && string.Equals(p["top"], "10px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))
            ), Times.Once);
            state.Div3.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "20px") && string.Equals(p["top"], "20px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))
            ), Times.Once);
        }

        [Test]
        public void Run_ShouldCreateVariable_GivenAssignment()
        {
            var state = new TestState();

            state.Scene.Run(@"
                ship = Image(assets/ship-0001.svg).set({position:absolute,left:10%,top:80%,width:100px,height:100px});
                ship.transition({top:20%,width:300px,height:300px,duration:1000});
                ship.transition({top:10%,transform:rotate(90deg),duration:500});
");

            state.Image1.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "position")),
                It.Is<string>(v => string.Equals(v, "absolute"))), Times.Once);

            state.Image1.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["top"], "20%") && string.Equals(p["width"], "300px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromSeconds(1))
            ), Times.Once);

            state.World.Verify(w => w.AddFutureEvent(
                It.Is<TemporalEvent>(t => t.TriggerAt == TimeSpan.FromSeconds(1))
            ), Times.Once);

            state.Image1.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["top"], "10%") && string.Equals(p["transform"], "rotate(90deg)")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(500))
            ), Times.Once);
        }

        [Test]
        public void Run_ShouldWait_GivenWaitCommand()
        {
            var state = new TestState();

            state.Scene.Run(@"
                Image(assets/elephant-sitting.png)
                    .set({position:absolute,opacity:0,left:90%,top:83%,width:200px,height:200px})
                    .wait(4000)
                    .transition({opacity:0.2,left:70%,duration:1000})
                    .wait(2000)
                    .set({left:50%});
            ");

            state.Image2.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "position")),
                It.Is<string>(v => string.Equals(v, "absolute"))), Times.Once);

            state.World.Verify(w => w.AddFutureEvent(
                It.Is<TemporalEvent>(t => t.TriggerAt == TimeSpan.FromSeconds(4))
            ), Times.Once);

            state.Image2.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["opacity"], "0.2") && string.Equals(p["left"], "70%")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromSeconds(1))
            ), Times.Once);

            state.World.Verify(w => w.AddFutureEvent(
                It.Is<TemporalEvent>(t => t.TriggerAt == TimeSpan.FromSeconds(7))
            ), Times.Once);

            state.Image2.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "position")),
                It.Is<string>(v => string.Equals(v, "absolute"))), Times.Once);
        }

        [Test]
        public void Run_ShouldWaitToCreateNodes_GivenAtSymbolCommand()
        {
            var state = new TestState();

            state.Scene.Run(@"
                @(4000);
                Image(assets/elephant-sitting.png)
                    .wait(1000)
                    .set({position:absolute,opacity:0,left:90%,top:83%,width:200px,height:200px});
            ");

            state.World.Verify(w => w.AddFutureEvent(
                It.Is<TemporalEvent>(t => t.TriggerAt == TimeSpan.FromSeconds(4))
            ), Times.Once);

            state.World.Verify(w => w.AddFutureEvent(
                It.Is<TemporalEvent>(t => t.TriggerAt == TimeSpan.FromSeconds(5))
            ), Times.Exactly(6));

            state.Image2.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "position")),
                It.Is<string>(v => string.Equals(v, "absolute"))), Times.Once);
        }

        [Test]
        public void Run_ShouldPerformCommands_GivenCommandsAreSpreadOverMultipleLines()
        {
            var state = new TestState();

            state.Scene.Run(@"
            elephant = Image(assets/elephant-sitting.png)
                .set({position:absolute,opacity:0,left:90%,top:83%,width:200px,height:200px})
                .transition({opacity:0.2,left:70%,duration:1000});
");

            state.Image2.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "position")),
                It.Is<string>(v => string.Equals(v, "absolute"))), Times.Once);

            state.Image2.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["opacity"], "0.2") && string.Equals(p["left"], "70%")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromSeconds(1))
            ), Times.Once);
        }

        [Test]
        public void Run_ShouldIgnoreComments_GivenCommentLines()
        {
            var state = new TestState();

            state.Scene.Run(@"
                // create elephant and set position
                elephant = Image(assets/elephant-sitting.png).set({position:absolute,opacity:0,left:90%,top:83%,width:200px,height:200px});
                // move the elephant
                elephant.wait(4000).transition({opacity:0.2,left:70%,duration:1000});
");

            state.Image2.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "position")),
                It.Is<string>(v => string.Equals(v, "absolute"))), Times.Once);

            state.World.Verify(w => w.AddFutureEvent(
                It.Is<TemporalEvent>(t => t.TriggerAt == TimeSpan.FromSeconds(4))
            ), Times.Once);

            state.Image2.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["opacity"], "0.2") && string.Equals(p["left"], "70%")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromSeconds(1))
            ), Times.Once);
        }

        [Test]
        public void Run_ShouldLoadNodesFromOtherAssembly_GivenUsingLine()
        {
            var state = new TestState();

            state.Scene.Run(@"
                using AutomationNodesTests;
                node = MyNode(my parameter).set({position:absolute});
");

            state.MyNode.Verify(d => d.SetProperty(
                It.Is<string>(n => string.Equals(n, "position")),
                It.Is<string>(v => string.Equals(v, "absolute"))), Times.Once);
        }


    }
}
