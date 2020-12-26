﻿using AutomationNodes.Core;
using AutomationNodes.Core.Compile;
using AutomationNodes.Nodes;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AutomationNodesTests
{
    public static class SceneEventExtensions
    {
        public static void ShouldBeCreateStatement(this CompiledStatement sceneStatement, string nodeName, Type type, TimeSpan triggerAt, params string[] parameters)
        {
            sceneStatement.Should().BeEquivalentTo(
                new SceneCreateStatement { NodeName = nodeName, Type = type, Parameters = parameters, TriggerAt = triggerAt }
            );
        }

        public static void ShouldBeCreateStatement(this CompiledStatement sceneStatement, Type type, TimeSpan triggerAt, params string[] parameters)
        {
            sceneStatement.Should().BeEquivalentTo(
                new SceneCreateStatement { Type = type, Parameters = parameters, TriggerAt = triggerAt },
                opt => opt.Excluding(su => su.NodeName)
            );
            (sceneStatement as SceneCreateStatement).NodeName.Should().NotBeNullOrEmpty();
        }

        public static void ShouldBeCreateFromClassStatement(this CompiledStatement sceneStatement, string className, Type type, TimeSpan triggerAt, params string[] parameters)
        {
            sceneStatement.Should().BeEquivalentTo(
                new SceneCreateStatement { Class = className, Type = type, Parameters = parameters, TriggerAt = triggerAt },
                opt => opt.Excluding(su => su.NodeName)
            );
        }

        public static void ShouldBeCreateFromClassStatement(this CompiledStatement sceneStatement, string nodeName, string className, Type type, TimeSpan triggerAt, params string[] parameters)
        {
            sceneStatement.Should().BeEquivalentTo(
                new SceneCreateStatement { NodeName = nodeName, Class = className, Type = type, Parameters = parameters, TriggerAt = triggerAt }
            );
        }

        public static void ShouldBeCreateChildStatement(this CompiledStatement sceneStatement, string nodeName, string parentNodeName, Type type, TimeSpan triggerAt, params string[] parameters)
        {
            sceneStatement.Should().BeEquivalentTo(
                new SceneCreateStatement { NodeName = nodeName, ParentNodeName = parentNodeName, Type = type, Parameters = parameters, TriggerAt = triggerAt }
            );
        }

        public static void ShouldBeSetPropertyStatement(this CompiledStatement sceneStatement, string nodeName, string propertyName, string propertyValue, TimeSpan triggerAt)
        {
            sceneStatement.Should().BeEquivalentTo(new SceneSetPropertyStatement {
                NodeName = nodeName, PropertyName = propertyName, PropertyValue = propertyValue, TriggerAt = triggerAt
            });
        }

        public static void ShouldBeSetPropertyStatement(this CompiledStatement sceneStatement, string propertyName, string propertyValue, TimeSpan triggerAt)
        {
            sceneStatement.Should().BeEquivalentTo(
                new SceneSetPropertyStatement { PropertyName = propertyName, PropertyValue = propertyValue, TriggerAt = triggerAt },
                opt => opt.Excluding(su => su.NodeName)
            );
        }

        public static void ShouldBeSetTransitionEvent(this CompiledStatement sceneStatement, string nodeName, Dictionary<string, string> transitionName, TimeSpan duration, TimeSpan triggerAt)
        {
            sceneStatement.Should().BeEquivalentTo(new SceneSetTransitionStatement {
                NodeName = nodeName,
                TransitionProperties = transitionName,
                Duration = duration,
                TriggerAt = triggerAt
            });
        }

        public static void ShouldBeSetTransitionEvent(this CompiledStatement sceneStatement, Dictionary<string, string> transitionName, TimeSpan duration, TimeSpan triggerAt)
        {
            sceneStatement.Should().BeEquivalentTo(new SceneSetTransitionStatement {
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
                var serviceProvider = new Mock<IServiceProvider>();
                var constructionModule = new ConstructionModule(serviceProvider.Object);
                var setFunctionModule = new SetFunctionModule(serviceProvider.Object);
                var transitionFunctionModule = new TransitionFunctionModule(serviceProvider.Object);
                var classModule = new ClassModule(serviceProvider.Object);
                var functionModule = new FunctionModule(serviceProvider.Object);
                var openingModule = new OpeningModule(constructionModule, setFunctionModule, transitionFunctionModule, classModule, functionModule);
                serviceProvider.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(IOpeningModule)))).Returns(openingModule);
                SceneCompiler = new SceneCompiler(new ScriptTokenizer(), openingModule);
            }
        }

        [Test]
        public void Run_ShouldCreateTextNode_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div();";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateTextNodeWithParameter_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div(1);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
        }

        [Test]
        public void Run_ShouldCreateTextNodeWithMultipleParameters_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div(1,abc,45.1deg);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1", "abc", "45.1deg");
        }

        [Test]
        public void Run_ShouldSetOneProperties_GivenSetCommand()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(2);
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            var nodeName = (events[0] as SceneCreateStatement).NodeName;
            events[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSetMultipleProperties_GivenSetCommand()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute,left:100px,top:100px])";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(4);
            var nodeName = (events[0] as SceneCreateStatement).NodeName;
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement(nodeName, "left", "100px", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyStatement(nodeName, "top", "100px", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSet_GivenSetCommandContainingBrackets()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute,transform:rotate(40.4.deg)])";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(3);
            var nodeName = (events[0] as SceneCreateStatement).NodeName;
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement(nodeName, "transform", "rotate(40.4.deg)", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSetTransition_GivenTransitionCommand()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute,left:100px,top:100px]).transition([left:0px,top:0px,duration:1000])";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(5);
            var nodeName = (events[0] as SceneCreateStatement).NodeName;
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement(nodeName, "left", "100px", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyStatement(nodeName, "top", "100px", TimeSpan.Zero);
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
            var nodeName = (events[0] as SceneCreateStatement).NodeName;
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement(nodeName, "left", "100px", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyStatement(nodeName, "top", "100px", TimeSpan.Zero);
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
            var node1Name = (events[0] as SceneCreateStatement).NodeName;
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            events[1].ShouldBeSetPropertyStatement(node1Name, "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement(node1Name, "left", "100px", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyStatement(node1Name, "top", "100px", TimeSpan.Zero);
            events[4].ShouldBeSetTransitionEvent(node1Name, new Dictionary<string, string> { { "left", "0px" }, { "top", "0px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);

            var node2Name = (events[5] as SceneCreateStatement).NodeName;
            events[5].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "2");
            events[6].ShouldBeSetPropertyStatement(node2Name, "position", "absolute", TimeSpan.Zero);
            events[7].ShouldBeSetPropertyStatement(node2Name, "left", "200px", TimeSpan.Zero);
            events[8].ShouldBeSetPropertyStatement(node2Name, "top", "200px", TimeSpan.Zero);
            events[9].ShouldBeSetTransitionEvent(node2Name, new Dictionary<string, string> { { "left", "10px" }, { "top", "10px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);

            var node3Name = (events[10] as SceneCreateStatement).NodeName;
            events[10].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "3");
            events[11].ShouldBeSetPropertyStatement(node3Name, "position", "absolute", TimeSpan.Zero);
            events[12].ShouldBeSetPropertyStatement(node3Name, "left", "300px", TimeSpan.Zero);
            events[13].ShouldBeSetPropertyStatement(node3Name, "top", "300px", TimeSpan.Zero);
            events[14].ShouldBeSetTransitionEvent(node3Name, new Dictionary<string, string> { { "left", "20px" }, { "top", "20px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateVariableWithParameter_GivenAssignment()
        {
            var state = new TestState();
            const string script = @"
                var ship = Image(assets/ship-0001.svg).set([position:absolute,left:10%,top:80%,width:100px,height:100px]);
                ship.transition([top:20%,width:300px,height:300px,duration:1000]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(7);
            events[0].ShouldBeCreateStatement("ship", typeof(Image), TimeSpan.Zero, "assets/ship-0001.svg");
            events[1].ShouldBeSetPropertyStatement("ship", "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement("ship", "left", "10%", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyStatement("ship", "top", "80%", TimeSpan.Zero);
            events[4].ShouldBeSetPropertyStatement("ship", "width", "100px", TimeSpan.Zero);
            events[5].ShouldBeSetPropertyStatement("ship", "height", "100px", TimeSpan.Zero);
            events[6].ShouldBeSetTransitionEvent("ship", new Dictionary<string, string> { { "top", "20%" }, { "width", "300px" }, { "height", "300px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldChainTransitionTimeSpans_GivenMultipleTransitions()
        {
            var state = new TestState();
            const string script = @"
                var ship = Image(assets/ship-0001.svg).set([position:absolute,left:10%,top:80%,width:100px,height:100px]);
                ship.transition([top:20%,width:300px,height:300px,duration:1000]);
                ship.transition([top:10%,transform:rotate(90deg),duration:500]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(8);
            events[0].ShouldBeCreateStatement("ship", typeof(Image), TimeSpan.Zero, "assets/ship-0001.svg");
            events[1].ShouldBeSetPropertyStatement("ship", "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement("ship", "left", "10%", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyStatement("ship", "top", "80%", TimeSpan.Zero);
            events[4].ShouldBeSetPropertyStatement("ship", "width", "100px", TimeSpan.Zero);
            events[5].ShouldBeSetPropertyStatement("ship", "height", "100px", TimeSpan.Zero);
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
            events[0].ShouldBeCreateStatement(typeof(Image), TimeSpan.Zero, "assets/elephant-sitting.png");
            events[1].ShouldBeSetPropertyStatement("position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement("opacity", "0", TimeSpan.Zero);
            events[3].ShouldBeSetPropertyStatement("left", "90%", TimeSpan.Zero);
            events[4].ShouldBeSetPropertyStatement("top", "83%", TimeSpan.Zero);
            events[5].ShouldBeSetPropertyStatement("width", "200px", TimeSpan.Zero);
            events[6].ShouldBeSetPropertyStatement("height", "200px", TimeSpan.Zero);
            events[7].ShouldBeSetTransitionEvent(new Dictionary<string, string> { { "opacity", "0.2" }, { "left", "70%" } },
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
            events[8].ShouldBeSetPropertyStatement("left", "50%", TimeSpan.FromSeconds(7));
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
            events[0].ShouldBeCreateStatement(typeof(Image), TimeSpan.FromSeconds(4), "assets/elephant-sitting.png");
            events[1].ShouldBeSetPropertyStatement("position", "absolute", TimeSpan.FromSeconds(5));
            events[2].ShouldBeSetPropertyStatement("opacity", "0", TimeSpan.FromSeconds(5));
            events[3].ShouldBeSetPropertyStatement("left", "90%", TimeSpan.FromSeconds(5));
            events[4].ShouldBeSetPropertyStatement("top", "83%", TimeSpan.FromSeconds(5));
            events[5].ShouldBeSetPropertyStatement("width", "200px", TimeSpan.FromSeconds(5));
            events[6].ShouldBeSetPropertyStatement("height", "200px", TimeSpan.FromSeconds(5));
        }

        [Test]
        public void Run_ShouldIgnoreComments_GivenCommentLines()
        {
            var state = new TestState();
            const string script = @"
                // create elephant and set position
                var elephant = Image(assets/elephant-sitting.png).set([position:absolute,opacity:0]);
                //move the elephant
                //after waiting
                elephant.wait(4000).transition([opacity:0.2,left:70%,duration:1000]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(4);
            events[0].ShouldBeCreateStatement("elephant", typeof(Image), TimeSpan.Zero, "assets/elephant-sitting.png");
            events[1].ShouldBeSetPropertyStatement("elephant", "position", "absolute", TimeSpan.Zero);
            events[2].ShouldBeSetPropertyStatement("elephant", "opacity", "0", TimeSpan.Zero);
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
                var node = MyNode(my parameter).set([position:absolute]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(2);
            events[0].ShouldBeCreateStatement("node", typeof(MyNode), TimeSpan.Zero, "my parameter");
            events[1].ShouldBeSetPropertyStatement("node", "position", "absolute", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateCustomNodeWithoutParameter_GivenNodeWithoutParameter()
        {
            var state = new TestState();
            const string script = @"
                using AutomationNodesTests;
                var node = MyNode().set([position:absolute]);";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(2);
            events[0].ShouldBeCreateStatement("node", typeof(MyNode), TimeSpan.Zero);
            events[1].ShouldBeSetPropertyStatement("node", "position", "absolute", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateFunctionStatement_GivenFunctionDefinition()
        {
            var state = new TestState();
            const string script = @"
            function myFunc() {
                Div(1).set([z-index:1]).transition([left:10px,top:10px,duration:1000]);
            };
            myFunc();
            myFunc();
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(6);
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            events[1].ShouldBeSetPropertyStatement("z-index", "1", TimeSpan.Zero);
            events[2].ShouldBeSetTransitionEvent(new Dictionary<string, string> { { "left", "10px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            events[3].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            events[4].ShouldBeSetPropertyStatement("z-index", "1", TimeSpan.Zero);
            events[5].ShouldBeSetTransitionEvent(new Dictionary<string, string> { { "left", "10px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateFunctionStatementWithParameters_GivenFunctionDefinition()
        {
            var state = new TestState();
            const string script = @"
            function myFunc(divNum,pos,movepos) {
                Div(%divNum%).set([left:%pos%]).transition([left:%movepos%,top:10px,duration:1000]);
            };
            myFunc(1,100px,25px);
            myFunc(2,200px);
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(6);
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            events[1].ShouldBeSetPropertyStatement("left", "100px", TimeSpan.Zero);
            events[2].ShouldBeSetTransitionEvent(new Dictionary<string, string> { { "left", "25px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            events[3].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "2" });
            events[4].ShouldBeSetPropertyStatement("left", "200px", TimeSpan.Zero);
            events[5].ShouldBeSetTransitionEvent(new Dictionary<string, string> { { "left", "" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateFunctionStatementWithCrazyWhitespace_GivenFunctionDefinition()
        {
            var state = new TestState();
            const string script = @"
            function myFunc ( divNum , pos , movepos ) {
                Div( %divNum% ) . set( [ left : %pos% ] ) . transition( [ left : %movepos% , top :10px, duration : 1000 ] ) ;
            };
            myFunc (1, 100px , 25px );
            myFunc ( 2, 200px) ;
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(6);
            events[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            events[1].ShouldBeSetPropertyStatement("left", "100px", TimeSpan.Zero);
            events[2].ShouldBeSetTransitionEvent(new Dictionary<string, string> { { "left", "25px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            events[3].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "2" });
            events[4].ShouldBeSetPropertyStatement("left", "200px", TimeSpan.Zero);
            events[5].ShouldBeSetTransitionEvent(new Dictionary<string, string> { { "left", "" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateClassStatement_GivenClassDefinition()
        {
            var state = new TestState();
            const string script = @"
            class Bird() {};
            Bird();
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateFromClassStatement("Bird", typeof(GenericNode), TimeSpan.Zero, new string[0]);
        }

        [Test]
        public void Run_ShouldCreateClassStatementWithConstructorParameters_GivenClassDefinition()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width,height) {};
            Bird(100px);
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateFromClassStatement("Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px" });
        }

        [Test]
        public void Run_ShouldCreateClassStatementWithConstructorParametersAndWhiteSpace_GivenClassDefinition()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width, height) { }
            Bird( 100px, 200px );
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(1);
            events[0].ShouldBeCreateFromClassStatement("Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
        }

        [Test]
        public void Run_ShouldCreateGenericNodeWithChildNodes_GivenClassUsage()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width,height) {
                var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
                var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%);
                var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%);
            };
            var myBird = Bird(100px,200px);
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(5);
            events[0].ShouldBeCreateFromClassStatement("myBird", "Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
            events[1].ShouldBeCreateChildStatement("myBird.body", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-body.png", "100px", "200px" });
            events[2].ShouldBeSetPropertyStatement("myBird.body", "z-index", "1", TimeSpan.Zero);
            events[3].ShouldBeCreateChildStatement("myBird.leftWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-left-wing.png", "100px", "200px" });
            events[4].ShouldBeCreateChildStatement("myBird.rightWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-right-wing.png", "100px", "200px" });
        }

        [Test]
        public void Run_ShouldSetProprertyForGenericNode_GivenClassUsage()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width,height) {
                var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
                var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%);
                var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%);
            };
            var myBird = Bird(100px,200px).set([left:500px,top:300px]);
            ";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(7);
            events[0].ShouldBeCreateFromClassStatement("myBird", "Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
            events[1].ShouldBeCreateChildStatement("myBird.body", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-body.png", "100px", "200px" });
            events[2].ShouldBeSetPropertyStatement("myBird.body", "z-index", "1", TimeSpan.Zero);
            events[3].ShouldBeCreateChildStatement("myBird.leftWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-left-wing.png", "100px", "200px" });
            events[4].ShouldBeCreateChildStatement("myBird.rightWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-right-wing.png", "100px", "200px" });
            events[5].ShouldBeSetPropertyStatement("myBird", "left", "500px", TimeSpan.Zero);
            events[6].ShouldBeSetPropertyStatement("myBird", "top", "300px", TimeSpan.Zero);
        }

        //[Test]
        public void Run_ShouldCreateCustomNodes_GivenNodeDefinition()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width, height) {
                var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
                var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%);
                var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%);
                flap(duration) {
                    leftWing.transition([transform:rotate(-80deg)]);
                    rightWing.transition([transform:rotate(80deg)]);
                    @%duration%
                    leftWing.transition([transform:rotate(0deg)]);
                    rightWing.transition([transform:rotate(0deg)]);
                }
            };";

            var events = state.SceneCompiler.Compile(script);

            events.Count.Should().Be(2);
            events[0].ShouldBeCreateStatement("node", typeof(MyNode), TimeSpan.Zero);
            events[1].ShouldBeSetPropertyStatement("node", "position", "absolute", TimeSpan.Zero);
        }
    }
}
