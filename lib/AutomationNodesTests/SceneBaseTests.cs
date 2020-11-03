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
            public MyNode(IWorld world) : base(world)
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

            public TestState()
            {
                Div1 = new Mock<INode>();
                Div2 = new Mock<INode>();
                Div3 = new Mock<INode>();
                World = new Mock<IWorld>();
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(DivNode)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "1")))).Returns(Div1.Object);
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(DivNode)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "2")))).Returns(Div2.Object);
                World.Setup(w => w.CreateNode(
                    It.Is<Type>(t => t == typeof(DivNode)),
                    It.Is<object[]>(p => string.Equals((string)p[0], "3")))).Returns(Div3.Object);

                Scene = new SceneBase(World.Object);
            }
        }

        [Test]
        public void Run_ShouldCreateTextNode_GivenDeclaration()
        {
            var state = new TestState();

            state.Scene.Run("DivNode(1)");

            state.World.Verify(w => w.CreateNode(
                It.Is<Type>(t => t == typeof(DivNode)),
                It.Is<object[]>(p => string.Equals((string)p[0], "1"))), Times.Once);
        }

        [Test]
        public void Run_ShouldSetProperties_GivenSetCommand()
        {
            var state = new TestState();

            state.Scene.Run("DivNode(1).set({position:absolute,left:100px,top:100px})");

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

            state.Scene.Run("DivNode(1).set({position:absolute,left:100px,top:100px}).transition({left:0px,top:0px,duration:1000})");

            state.Div1.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "0px") && string.Equals(p["top"], "0px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))), Times.Once);
        }

        [Test]
        public void Run_ShouldSetTransition_GivenTransitionCommandWithWhiteSpace()
        {
            var state = new TestState();

            state.Scene.Run(@"
DivNode(1).set({position:absolute,left:100px,top:100px}).transition({left:0px,top:0px,duration:1000})");

            state.Div1.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "0px") && string.Equals(p["top"], "0px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))), Times.Once);
        }

        [Test]
        public void Run_ShouldProcessMultipleStatements_GivenMultiLineScript()
        {
            var state = new TestState();

            state.Scene.Run(@"
DivNode(1).set({position:absolute,left:100px,top:100px}).transition({left:0px,top:0px,duration:1000});
DivNode(2).set({position:absolute,left:200px,top:200px}).transition({left:10px,top:10px,duration:1000});
DivNode(3).set({position:absolute,left:300px,top:300px}).transition({left:20px,top:20px,duration:1000});
");

            state.Div1.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "0px") && string.Equals(p["top"], "0px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))), Times.Once);
            state.Div2.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "10px") && string.Equals(p["top"], "10px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))), Times.Once);
            state.Div3.Verify(d => d.SetTransition(
                It.Is<Dictionary<string, string>>(p => string.Equals(p["left"], "20px") && string.Equals(p["top"], "20px")),
                It.Is<TimeSpan>(t => t == TimeSpan.FromMilliseconds(1000))), Times.Once);
        }
    }
}
