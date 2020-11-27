using AutomationNodes.Core;
using AutomationNodes.Nodes;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AutomationNodesTests
{
    public static class SceneEventExtensions
    {
        public static void ShouldBeCreateEvent(this SceneEvent sceneEvent, string nodeName, Type type, TimeSpan triggerAt, params string[] parameters)
        {
            sceneEvent.Should().BeEquivalentTo<SceneCreateEvent>(
                new SceneCreateEvent { NodeName = nodeName, Type = type, Parameters = parameters, TriggerAt = triggerAt }
            );
        }

        public static void ShouldBeCreateEvent(this SceneEvent sceneEvent, Type type, TimeSpan triggerAt, params string[] parameters)
        {
            sceneEvent.Should().BeEquivalentTo<SceneCreateEvent>(
                new SceneCreateEvent { Type = type, Parameters = parameters, TriggerAt = triggerAt },
                opt => opt.Excluding(su => su.NodeName)
            );
        }

        public static void ShouldBeSetPropertyEvent(this SceneEvent sceneEvent, string nodeName, string propertyName, string propertyValue, TimeSpan triggerAt)
        {
            sceneEvent.Should().BeEquivalentTo(new SceneSetPropertyEvent {
                NodeName = nodeName, PropertyName = propertyName, PropertyValue = propertyValue, TriggerAt = triggerAt
            });
        }

        public static void ShouldBeSetPropertyEvent(this SceneEvent sceneEvent, string propertyName, string propertyValue, TimeSpan triggerAt)
        {
            sceneEvent.Should().BeEquivalentTo(
                new SceneSetPropertyEvent { PropertyName = propertyName, PropertyValue = propertyValue, TriggerAt = triggerAt },
                opt => opt.Excluding(su => su.NodeName)
            );
        }

        public static void ShouldBeSetTransitionEvent(this SceneEvent sceneEvent, string nodeName, Dictionary<string, string> transitionName, TimeSpan duration, TimeSpan triggerAt)
        {
            sceneEvent.Should().BeEquivalentTo(new SceneSetTransitionEvent {
                NodeName = nodeName,
                TransitionProperties = transitionName,
                Duration = duration,
                TriggerAt = triggerAt
            });
        }

        public static void ShouldBeSetTransitionEvent(this SceneEvent sceneEvent, Dictionary<string, string> transitionName, TimeSpan duration, TimeSpan triggerAt)
        {
            sceneEvent.Should().BeEquivalentTo(new SceneSetTransitionEvent {
                TransitionProperties = transitionName,
                Duration = duration,
                TriggerAt = triggerAt
            }, opt => opt.Excluding(su => su.NodeName));
        }
    }

    public class SceneCompilerTests
    {
        private class TestState
        {
            public SceneCompiler SceneCompiler { get; }

            public TestState()
            {
                SceneCompiler = new SceneCompiler();
            }
        }

        [Test]
        public void Run_ShouldCreateTextNode_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div()";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateTextNodeWithParameter_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div(1)";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero, "1");
        }

        [Test]
        public void Run_ShouldCreateTextNodeWithMultipleParameters_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div(1,abc,45.1deg)";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero, "1", "abc", "45.1deg");
        }

        [Test]
        public void Run_ShouldSetProperties_GivenSetCommand()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute,left:100px,top:100px])";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(4);
            var nodeName = events[0].NodeName;
            events[0].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyEvent(nodeName, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyEvent(nodeName, "left", "100px", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyEvent(nodeName, "top", "100px", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSetTransition_GivenTransitionCommand()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute,left:100px,top:100px]).transition([left:0px,top:0px,duration:1000])";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(5);
            var nodeName = events[0].NodeName;
            events[0].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyEvent(nodeName, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyEvent(nodeName, "left", "100px", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyEvent(nodeName, "top", "100px", TimeSpan.Zero);
            events[4].ShouldBeSetTransitionEvent(nodeName, new Dictionary<string, string> { { "left", "0px" }, { "top", "0px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSetTransition_GivenTransitionCommandWithWhiteSpace()
        {
            var state = new TestState();
            const string script = @"
                Div(1)
                    .set([position:absolute,left:100px,top:100px])
                    .transition([left:0px,top:0px,duration:1000]);
";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(5);
            var nodeName = events[0].NodeName;
            events[0].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyEvent(nodeName, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyEvent(nodeName, "left", "100px", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyEvent(nodeName, "top", "100px", TimeSpan.Zero);
            events[4].ShouldBeSetTransitionEvent(nodeName, new Dictionary<string, string> { { "left", "0px" }, { "top", "0px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldProcessMultipleStatements_GivenMultiLineScript()
        {
            var state = new TestState();
            const string script = @"
                    Div(1).set([position:absolute,left:100px,top:100px]).transition([left:0px,top:0px,duration:1000]);
                    Div(2).set([position:absolute,left:200px,top:200px]).transition([left:10px,top:10px,duration:1000]);
                    Div(3).set([position:absolute,left:300px,top:300px]).transition([left:20px,top:20px,duration:1000]);
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(15);
            var node1Name = events[0].NodeName;
            events[0].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyEvent(node1Name, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyEvent(node1Name, "left", "100px", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyEvent(node1Name, "top", "100px", TimeSpan.Zero);
            events[4].ShouldBeSetTransitionEvent(node1Name, new Dictionary<string, string> { { "left", "0px" }, { "top", "0px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);

            var node2Name = events[5].NodeName;
            events[5].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero, "2");
            events[6].ShouldBeSetPropertyEvent(node2Name, "position", "absolute", TimeSpan.Zero);
            events[7].ShouldBeSetPropertyEvent(node2Name, "left", "200px", TimeSpan.Zero);
            events[8].ShouldBeSetPropertyEvent(node2Name, "top", "200px", TimeSpan.Zero);
            events[9].ShouldBeSetTransitionEvent(node2Name, new Dictionary<string, string> { { "left", "10px" }, { "top", "10px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);

            var node3Name = events[10].NodeName;
            events[10].ShouldBeCreateEvent(typeof(Div), TimeSpan.Zero, "3");
            events[11].ShouldBeSetPropertyEvent(node3Name, "position", "absolute", TimeSpan.Zero);
            events[12].ShouldBeSetPropertyEvent(node3Name, "left", "300px", TimeSpan.Zero);
            events[13].ShouldBeSetPropertyEvent(node3Name, "top", "300px", TimeSpan.Zero);
            events[14].ShouldBeSetTransitionEvent(node3Name, new Dictionary<string, string> { { "left", "20px" }, { "top", "20px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateVariableWithParameter_GivenAssignment()
        {
            var state = new TestState();
            const string script = @"
                ship = Image(assets/ship-0001.svg).set([position:absolute,left:10%,top:80%,width:100px,height:100px]);
                ship.transition([top:20%,width:300px,height:300px,duration:1000]);
                ship.transition([top:10%,transform:rotate(90deg),duration:500]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(8);
            events[0].ShouldBeCreateEvent("ship", typeof(Image), TimeSpan.Zero, "assets/ship-0001.svg");
            events[1].ShouldBeSetPropertyEvent("ship", "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyEvent("ship", "left", "10%", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyEvent("ship", "top", "80%", TimeSpan.Zero);
            events[4].ShouldBeSetPropertyEvent("ship", "width", "100px", TimeSpan.Zero);
            events[5].ShouldBeSetPropertyEvent("ship", "height", "100px", TimeSpan.Zero);
            events[6].ShouldBeSetTransitionEvent("ship", new Dictionary<string, string> { { "top", "20%" }, { "width", "300px" }, { "height", "300px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);
            events[7].ShouldBeSetTransitionEvent("ship", new Dictionary<string, string> { { "top", "10%" }, { "transform", "rotate(90deg)" } },
                TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1));
        }

        [Test]
        public void Run_ShouldIncreaseTimeToEventTrigger_GivenWaitCommand()
        {
            var state = new TestState();
            const string script = @"
                Image(assets/elephant-sitting.png)
                    .set([position:absolute,opacity:0,left:90%,top:83%,width:200px,height:200px])
                    .wait(4000)
                    .transition([opacity:0.2,left:70%,duration:1000])
                    .wait(2000)
                    .set([left:50%]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(9);
            events[0].ShouldBeCreateEvent(typeof(Image), TimeSpan.Zero, "assets/elephant-sitting.png");
            events[1].ShouldBeSetPropertyEvent("position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyEvent("opacity", "0", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyEvent("left", "90%", TimeSpan.Zero);
            events[4].ShouldBeSetPropertyEvent("top", "83%", TimeSpan.Zero);
            events[5].ShouldBeSetPropertyEvent("width", "200px", TimeSpan.Zero);
            events[6].ShouldBeSetPropertyEvent("height", "200px", TimeSpan.Zero);
            events[7].ShouldBeSetTransitionEvent(new Dictionary<string, string> { { "opacity", "0.2" }, { "left", "70%" } },
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
            events[8].ShouldBeSetPropertyEvent("left", "50%", TimeSpan.FromSeconds(7));
        }

        [Test]
        public void Run_ShouldWaitToCreateNodes_GivenAtSymbolCommand()
        {
            var state = new TestState();
            const string script = @"
                @(4000);
                Image(assets/elephant-sitting.png)
                    .wait(1000)
                    .set([position:absolute,opacity:0,left:90%,top:83%,width:200px,height:200px]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(7);
            events[0].ShouldBeCreateEvent(typeof(Image), TimeSpan.FromSeconds(4), "assets/elephant-sitting.png");
            events[1].ShouldBeSetPropertyEvent("position", "absolute", TimeSpan.FromSeconds(5));
            events[2].ShouldBeSetPropertyEvent("opacity", "0", TimeSpan.FromSeconds(5));
            events[3].ShouldBeSetPropertyEvent("left", "90%", TimeSpan.FromSeconds(5));
            events[4].ShouldBeSetPropertyEvent("top", "83%", TimeSpan.FromSeconds(5));
            events[5].ShouldBeSetPropertyEvent("width", "200px", TimeSpan.FromSeconds(5));
            events[6].ShouldBeSetPropertyEvent("height", "200px", TimeSpan.FromSeconds(5));
        }

        [Test]
        public void Run_ShouldIgnoreComments_GivenCommentLines()
        {
            var state = new TestState();
            const string script = @"
                // create elephant and set position
                elephant = Image(assets/elephant-sitting.png).set([position:absolute,opacity:0]);
                // move the elephant
                elephant.wait(4000).transition([opacity:0.2,left:70%,duration:1000]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(4);
            events[0].ShouldBeCreateEvent("elephant", typeof(Image), TimeSpan.Zero, "assets/elephant-sitting.png");
            events[1].ShouldBeSetPropertyEvent("elephant", "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyEvent("elephant", "opacity", "0", TimeSpan.Zero);
            events[3].ShouldBeSetTransitionEvent("elephant", new Dictionary<string, string> { { "opacity", "0.2" }, { "left", "70%" } },
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
        }

        public class MyNode : Div, INode
        {
            public override void OnCreate(object[] parameters)
            {
                base.OnCreate(parameters);
            }
        }

        [Test]
        public void Run_ShouldLoadNodesFromOtherAssembly_GivenUsingLine()
        {
            var state = new TestState();
            const string script = @"
                using AutomationNodesTests;
                node = MyNode(my parameter).set([position:absolute]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(2);
            events[0].ShouldBeCreateEvent("node", typeof(MyNode), TimeSpan.Zero, "my parameter");
            events[1].ShouldBeSetPropertyEvent("node", "position", "absolute", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateCustomNodeWithoutParameter_GivenNodeWithoutParameter()
        {
            var state = new TestState();
            const string script = @"
                using AutomationNodesTests;
                node = MyNode().set([position:absolute]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(2);
            events[0].ShouldBeCreateEvent("node", typeof(MyNode), TimeSpan.Zero);
            events[1].ShouldBeSetPropertyEvent("node", "position", "absolute", TimeSpan.Zero);
        }
    }
}
