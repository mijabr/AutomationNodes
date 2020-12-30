using AutomationNodes.Core;
using AutomationNodes.Core.Compile;
using AutomationNodes.Nodes;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AutomationNodesTests
{
    public static class CompiledStatementExtensions
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

        public static void ShouldBeSetTransitionStatement(this CompiledStatement sceneStatement, string nodeName, Dictionary<string, string> transitionName, TimeSpan duration, TimeSpan triggerAt)
        {
            sceneStatement.Should().BeEquivalentTo(new SceneSetTransitionStatement {
                NodeName = nodeName,
                TransitionProperties = transitionName,
                Duration = duration,
                TriggerAt = triggerAt
            });
        }

        public static void ShouldBeSetTransitionStatement(this CompiledStatement sceneStatement, Dictionary<string, string> transitionName, TimeSpan duration, TimeSpan triggerAt)
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
                var parameterModule = new ParameterModule();
                var setFunctionModule = new SetFunctionModule(serviceProvider.Object);
                var transitionFunctionModule = new TransitionFunctionModule(serviceProvider.Object);
                var classModule = new ClassModule(serviceProvider.Object);
                var functionModule = new FunctionModule(serviceProvider.Object);
                var openingModule = new OpeningModule(constructionModule, setFunctionModule, transitionFunctionModule, classModule, functionModule);
                serviceProvider.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(IOpeningModule)))).Returns(openingModule);
                serviceProvider.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(IConstructionModule)))).Returns(constructionModule);
                serviceProvider.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(IParameterModule)))).Returns(parameterModule);
                serviceProvider.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(IFunctionModule)))).Returns(functionModule);
                serviceProvider.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(IClassModule)))).Returns(classModule);
                serviceProvider.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(ISetFunctionModule)))).Returns(setFunctionModule);
                serviceProvider.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(ITransitionFunctionModule)))).Returns(transitionFunctionModule);
                SceneCompiler = new SceneCompiler(new ScriptTokenizer(), openingModule);
            }
        }

        [Test]
        public void Run_ShouldCreateTextNode_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div();";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(1);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateTextNodeWithParameter_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div(1);";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(1);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
        }

        [Test]
        public void Run_ShouldThrowError_GivenUnknownTypeToken()
        {
            var state = new TestState();
            const string script = "Something(1);";

            Action action = () => state.SceneCompiler.Compile(script);

            action.Should().Throw<Exception>().WithMessage("Unknown function, class or type 'something'");
        }

        [Test]
        public void Run_ShouldCreateTextNodeWithMultipleParameters_GivenDeclaration()
        {
            var state = new TestState();
            const string script = "Div(1,abc,45.1deg);";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(1);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1", "abc", "45.1deg");
        }

        [Test]
        public void Run_ShouldSetOneProperties_GivenSetCommand()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute]);";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(2);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            var nodeName = (statements[0] as SceneCreateStatement).NodeName;
            statements[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSetMultipleProperties_GivenSetCommand()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute,left:100px,top:100px])";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(4);
            var nodeName = (statements[0] as SceneCreateStatement).NodeName;
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            statements[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement(nodeName, "left", "100px", TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement(nodeName, "top", "100px", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSet_GivenSetCommandContainingBrackets()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute,transform:rotate(40.4.deg)])";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(3);
            var nodeName = (statements[0] as SceneCreateStatement).NodeName;
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            statements[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement(nodeName, "transform", "rotate(40.4.deg)", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSetTransition_GivenTransitionCommand()
        {
            var state = new TestState();
            const string script = "Div(1).set([position:absolute,left:100px,top:100px]).transition([left:0px,top:0px,duration:1000])";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(5);
            var nodeName = (statements[0] as SceneCreateStatement).NodeName;
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            statements[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement(nodeName, "left", "100px", TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement(nodeName, "top", "100px", TimeSpan.Zero);
            statements[4].ShouldBeSetTransitionStatement(nodeName, new Dictionary<string, string> { { "left", "0px" }, { "top", "0px" } },
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(5);
            var nodeName = (statements[0] as SceneCreateStatement).NodeName;
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            statements[1].ShouldBeSetPropertyStatement(nodeName, "position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement(nodeName, "left", "100px", TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement(nodeName, "top", "100px", TimeSpan.Zero);
            statements[4].ShouldBeSetTransitionStatement(nodeName, new Dictionary<string, string> { { "left", "0px" }, { "top", "0px" } },
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(15);
            var node1Name = (statements[0] as SceneCreateStatement).NodeName;
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "1");
            statements[1].ShouldBeSetPropertyStatement(node1Name, "position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement(node1Name, "left", "100px", TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement(node1Name, "top", "100px", TimeSpan.Zero);
            statements[4].ShouldBeSetTransitionStatement(node1Name, new Dictionary<string, string> { { "left", "0px" }, { "top", "0px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);

            var node2Name = (statements[5] as SceneCreateStatement).NodeName;
            statements[5].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "2");
            statements[6].ShouldBeSetPropertyStatement(node2Name, "position", "absolute", TimeSpan.Zero);
            statements[7].ShouldBeSetPropertyStatement(node2Name, "left", "200px", TimeSpan.Zero);
            statements[8].ShouldBeSetPropertyStatement(node2Name, "top", "200px", TimeSpan.Zero);
            statements[9].ShouldBeSetTransitionStatement(node2Name, new Dictionary<string, string> { { "left", "10px" }, { "top", "10px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);

            var node3Name = (statements[10] as SceneCreateStatement).NodeName;
            statements[10].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, "3");
            statements[11].ShouldBeSetPropertyStatement(node3Name, "position", "absolute", TimeSpan.Zero);
            statements[12].ShouldBeSetPropertyStatement(node3Name, "left", "300px", TimeSpan.Zero);
            statements[13].ShouldBeSetPropertyStatement(node3Name, "top", "300px", TimeSpan.Zero);
            statements[14].ShouldBeSetTransitionStatement(node3Name, new Dictionary<string, string> { { "left", "20px" }, { "top", "20px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateVariableWithParameter_GivenAssignment()
        {
            var state = new TestState();
            const string script = @"
                var ship = Image(assets/ship-0001.svg).set([position:absolute,left:10%,top:80%,width:100px,height:100px]);
                ship.transition([top:20%,width:300px,height:300px,duration:1000]);";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(7);
            statements[0].ShouldBeCreateStatement("ship", typeof(Image), TimeSpan.Zero, "assets/ship-0001.svg");
            statements[1].ShouldBeSetPropertyStatement("ship", "position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement("ship", "left", "10%", TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement("ship", "top", "80%", TimeSpan.Zero);
            statements[4].ShouldBeSetPropertyStatement("ship", "width", "100px", TimeSpan.Zero);
            statements[5].ShouldBeSetPropertyStatement("ship", "height", "100px", TimeSpan.Zero);
            statements[6].ShouldBeSetTransitionStatement("ship", new Dictionary<string, string> { { "top", "20%" }, { "width", "300px" }, { "height", "300px" } },
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(8);
            statements[0].ShouldBeCreateStatement("ship", typeof(Image), TimeSpan.Zero, "assets/ship-0001.svg");
            statements[1].ShouldBeSetPropertyStatement("ship", "position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement("ship", "left", "10%", TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement("ship", "top", "80%", TimeSpan.Zero);
            statements[4].ShouldBeSetPropertyStatement("ship", "width", "100px", TimeSpan.Zero);
            statements[5].ShouldBeSetPropertyStatement("ship", "height", "100px", TimeSpan.Zero);
            statements[6].ShouldBeSetTransitionStatement("ship", new Dictionary<string, string> { { "top", "20%" }, { "width", "300px" }, { "height", "300px" } },
                TimeSpan.FromSeconds(1), TimeSpan.Zero);
            statements[7].ShouldBeSetTransitionStatement("ship", new Dictionary<string, string> { { "top", "10%" }, { "transform", "rotate(90deg)" } },
                TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1));
        }

        [Test]
        public void Run_ShouldIncreaseTimeToTrigger_GivenWaitCommand()
        {
            var state = new TestState();
            const string script = @"
                Image(assets/elephant-sitting.png)
                    .set([position:absolute,opacity:0,left:90%,top:83%,width:200px,height:200px])
                    .wait(4000)
                    .transition([opacity:0.2,left:70%,duration:1000])
                    .wait(2000)
                    .set([left:50%]);";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(9);
            statements[0].ShouldBeCreateStatement(typeof(Image), TimeSpan.Zero, "assets/elephant-sitting.png");
            statements[1].ShouldBeSetPropertyStatement("position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement("opacity", "0", TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement("left", "90%", TimeSpan.Zero);
            statements[4].ShouldBeSetPropertyStatement("top", "83%", TimeSpan.Zero);
            statements[5].ShouldBeSetPropertyStatement("width", "200px", TimeSpan.Zero);
            statements[6].ShouldBeSetPropertyStatement("height", "200px", TimeSpan.Zero);
            statements[7].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "opacity", "0.2" }, { "left", "70%" } },
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
            statements[8].ShouldBeSetPropertyStatement("left", "50%", TimeSpan.FromSeconds(7));
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(7);
            statements[0].ShouldBeCreateStatement(typeof(Image), TimeSpan.FromSeconds(4), "assets/elephant-sitting.png");
            statements[1].ShouldBeSetPropertyStatement("position", "absolute", TimeSpan.FromSeconds(5));
            statements[2].ShouldBeSetPropertyStatement("opacity", "0", TimeSpan.FromSeconds(5));
            statements[3].ShouldBeSetPropertyStatement("left", "90%", TimeSpan.FromSeconds(5));
            statements[4].ShouldBeSetPropertyStatement("top", "83%", TimeSpan.FromSeconds(5));
            statements[5].ShouldBeSetPropertyStatement("width", "200px", TimeSpan.FromSeconds(5));
            statements[6].ShouldBeSetPropertyStatement("height", "200px", TimeSpan.FromSeconds(5));
        }

        [Test]
        public void Run_ShouldResetNodeTimesToSceneTime_GivenAtSymbolCommand()
        {
            var state = new TestState();
            const string script = @"
                var elephant = Image(assets/elephant-sitting.png).set([left:90%]).transition([left:80%,duration:3000]);
                @(7000);
                elephant.set([left:50%]);";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(4);
            statements[0].ShouldBeCreateStatement(typeof(Image), TimeSpan.Zero, "assets/elephant-sitting.png");
            statements[1].ShouldBeSetPropertyStatement("left", "90%", TimeSpan.Zero);
            statements[2].ShouldBeSetTransitionStatement("elephant", new Dictionary<string, string> { { "left", "80%" } }, TimeSpan.FromSeconds(3), TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement("left", "50%", TimeSpan.FromSeconds(7));
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(4);
            statements[0].ShouldBeCreateStatement("elephant", typeof(Image), TimeSpan.Zero, "assets/elephant-sitting.png");
            statements[1].ShouldBeSetPropertyStatement("elephant", "position", "absolute", TimeSpan.Zero);
            statements[2].ShouldBeSetPropertyStatement("elephant", "opacity", "0", TimeSpan.Zero);
            statements[3].ShouldBeSetTransitionStatement("elephant", new Dictionary<string, string> { { "opacity", "0.2" }, { "left", "70%" } },
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(2);
            statements[0].ShouldBeCreateStatement("node", typeof(MyNode), TimeSpan.Zero, "my parameter");
            statements[1].ShouldBeSetPropertyStatement("node", "position", "absolute", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateCustomNodeWithoutParameter_GivenNodeWithoutParameter()
        {
            var state = new TestState();
            const string script = @"
                using AutomationNodesTests;
                var node = MyNode().set([position:absolute]);";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(2);
            statements[0].ShouldBeCreateStatement("node", typeof(MyNode), TimeSpan.Zero);
            statements[1].ShouldBeSetPropertyStatement("node", "position", "absolute", TimeSpan.Zero);
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(6);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            statements[1].ShouldBeSetPropertyStatement("z-index", "1", TimeSpan.Zero);
            statements[2].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "10px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            statements[3].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            statements[4].ShouldBeSetPropertyStatement("z-index", "1", TimeSpan.Zero);
            statements[5].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "10px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(6);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            statements[1].ShouldBeSetPropertyStatement("left", "100px", TimeSpan.Zero);
            statements[2].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "25px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            statements[3].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "2" });
            statements[4].ShouldBeSetPropertyStatement("left", "200px", TimeSpan.Zero);
            statements[5].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldAllowFunctionToModifyNodes_GivenFunctionDefinition()
        {
            var state = new TestState();
            const string script = @"
            var myNode = Div(1);
            function myFunc(pos,movepos) {
               myNode.set([left:%pos%]).transition([left:%movepos%,top:10px,duration:1000]);
            };
            myFunc(100px,25px);
            myFunc(200px);
            myFunc(300px,50px);
            ";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(7);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            statements[1].ShouldBeSetPropertyStatement("left", "100px", TimeSpan.Zero);
            statements[2].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "25px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement("left", "200px", TimeSpan.FromSeconds(1));
            statements[4].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            statements[5].ShouldBeSetPropertyStatement("left", "300px", TimeSpan.FromSeconds(2));
            statements[6].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "50px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        }

        [Test]
        public void Run_ShouldAllowAtFunctionToModifyNodes_GivenFunctionDefinition()
        {
            var state = new TestState();
            const string script = @"
            var myNode = Div(1);
            function myFunc(pos,movepos) {
               myNode.set([left:%pos%]).transition([left:%movepos%,top:10px,duration:1000]);
            };
            myFunc(100px,25px);
            @(3000);
            myFunc(200px);
            myFunc(200px);
            ";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(7);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            statements[1].ShouldBeSetPropertyStatement("left", "100px", TimeSpan.Zero);
            statements[2].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "25px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            statements[3].ShouldBeSetPropertyStatement("left", "200px", TimeSpan.FromSeconds(3));
            statements[4].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
            statements[5].ShouldBeSetPropertyStatement("left", "200px", TimeSpan.FromSeconds(4));
            statements[6].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(6);
            statements[0].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "1" });
            statements[1].ShouldBeSetPropertyStatement("left", "100px", TimeSpan.Zero);
            statements[2].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "25px" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            statements[3].ShouldBeCreateStatement(typeof(Div), TimeSpan.Zero, new string[] { "2" });
            statements[4].ShouldBeSetPropertyStatement("left", "200px", TimeSpan.Zero);
            statements[5].ShouldBeSetTransitionStatement(new Dictionary<string, string> { { "left", "" }, { "top", "10px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldCreateClassStatement_GivenClassDefinition()
        {
            var state = new TestState();
            const string script = @"
            class Bird() {};
            Bird();
            ";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(1);
            statements[0].ShouldBeCreateFromClassStatement("Bird", typeof(GenericNode), TimeSpan.Zero, new string[0]);
        }

        [Test]
        public void Run_ShouldCreateClassStatementWithConstructorParameters_GivenClassDefinition()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width,height) {};
            Bird(100px);
            ";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(1);
            statements[0].ShouldBeCreateFromClassStatement("Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px" });
        }

        [Test]
        public void Run_ShouldCreateClassStatementWithConstructorParametersAndWhiteSpace_GivenClassDefinition()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width, height) { }
            Bird( 100px, 200px );
            ";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(1);
            statements[0].ShouldBeCreateFromClassStatement("Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
        }

        [Test]
        public void Run_ShouldCreateClassNodeWithChildNodes_GivenClassUsage()
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(5);
            statements[0].ShouldBeCreateFromClassStatement("myBird", "Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
            statements[1].ShouldBeCreateChildStatement("myBird-body", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-body.png", "100px", "200px" });
            statements[2].ShouldBeSetPropertyStatement("myBird-body", "z-index", "1", TimeSpan.Zero);
            statements[3].ShouldBeCreateChildStatement("myBird-leftWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-left-wing.png", "100px", "200px" });
            statements[4].ShouldBeCreateChildStatement("myBird-rightWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-right-wing.png", "100px", "200px" });
        }

        [Test]
        public void Run_ShouldSetProprertyForClassNode_GivenClassUsage()
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(7);
            statements[0].ShouldBeCreateFromClassStatement("myBird", "Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
            statements[1].ShouldBeCreateChildStatement("myBird-body", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-body.png", "100px", "200px" });
            statements[2].ShouldBeSetPropertyStatement("myBird-body", "z-index", "1", TimeSpan.Zero);
            statements[3].ShouldBeCreateChildStatement("myBird-leftWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-left-wing.png", "100px", "200px" });
            statements[4].ShouldBeCreateChildStatement("myBird-rightWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-right-wing.png", "100px", "200px" });
            statements[5].ShouldBeSetPropertyStatement("myBird", "left", "500px", TimeSpan.Zero);
            statements[6].ShouldBeSetPropertyStatement("myBird", "top", "300px", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldSetTransitionForClassNode_GivenClassUsage()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width,height) {
                var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
                var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%).transition([transform:rotate(-80deg),duration:300]).transition([transform:rotate(0deg),duration:300]);
                var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%).transition([transform:rotate(80deg),duration:300]).transition([transform:rotate(0deg),duration:300]);
            };
            var myBird = Bird(100px,200px).set([left:500px,top:300px]).transition([left:200px,duration:1000]);
            ";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(12);
            statements[0].ShouldBeCreateFromClassStatement("myBird", "Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
            statements[1].ShouldBeCreateChildStatement("myBird-body", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-body.png", "100px", "200px" });
            statements[2].ShouldBeSetPropertyStatement("myBird-body", "z-index", "1", TimeSpan.Zero);
            statements[3].ShouldBeCreateChildStatement("myBird-leftWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-left-wing.png", "100px", "200px" });
            statements[4].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(-80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.Zero);
            statements[5].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300));
            statements[6].ShouldBeCreateChildStatement("myBird-rightWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-right-wing.png", "100px", "200px" });
            statements[7].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.Zero);
            statements[8].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300));
            statements[9].ShouldBeSetPropertyStatement("myBird", "left", "500px", TimeSpan.Zero);
            statements[10].ShouldBeSetPropertyStatement("myBird", "top", "300px", TimeSpan.Zero);
            statements[11].ShouldBeSetTransitionStatement("myBird", new Dictionary<string, string> { { "left", "200px" } }, TimeSpan.FromSeconds(1), TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldMoveClassUsingFunction_GivenClassFunctionUsage()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width,height) {
                var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
                var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%);
                var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%);
            };
            var myBird = Bird(100px,200px).set([left:500px,top:300px]);
            function flap() {
                myBird-leftWing.transition([transform:rotate(-80deg),duration:300]).transition([transform:rotate(0deg),duration:300]);
                myBird-rightWing.transition([transform:rotate(80deg),duration:300]).transition([transform:rotate(0deg),duration:300]);
                myBird-body.set([color:red]);
            };
            flap();
            flap();
            flap();
            ";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(22);
            statements[0].ShouldBeCreateFromClassStatement("myBird", "Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
            statements[1].ShouldBeCreateChildStatement("myBird-body", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-body.png", "100px", "200px" });
            statements[2].ShouldBeSetPropertyStatement("myBird-body", "z-index", "1", TimeSpan.Zero);
            statements[3].ShouldBeCreateChildStatement("myBird-leftWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-left-wing.png", "100px", "200px" });
            statements[4].ShouldBeCreateChildStatement("myBird-rightWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-right-wing.png", "100px", "200px" });
            statements[5].ShouldBeSetPropertyStatement("myBird", "left", "500px", TimeSpan.Zero);
            statements[6].ShouldBeSetPropertyStatement("myBird", "top", "300px", TimeSpan.Zero);
            statements[7].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(-80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.Zero);
            statements[8].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300));
            statements[9].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.Zero);
            statements[10].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300));
            statements[11].ShouldBeSetPropertyStatement("myBird-body", "color", "red", TimeSpan.Zero);
            statements[12].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(-80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(600));
            statements[13].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(900));
            statements[14].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(600));
            statements[15].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(900));
            statements[16].ShouldBeSetPropertyStatement("myBird-body", "color", "red", TimeSpan.Zero);
            statements[17].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(-80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1200));
            statements[18].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1500));
            statements[19].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1200));
            statements[20].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1500));
            statements[21].ShouldBeSetPropertyStatement("myBird-body", "color", "red", TimeSpan.Zero);
        }

        [Test]
        public void Run_ShouldMoveClassUsingClassFunction_GivenFunctionInsideClass()
        {
            var state = new TestState();
            const string script = @"
            class Bird(width,height) {
                var body = Image(assets/flying-bird-body.png,%width%,%height%).set([z-index:1]);
                var leftWing = Image(assets/flying-bird-left-wing.png,%width%,%height%);
                var rightWing = Image(assets/flying-bird-right-wing.png,%width%,%height%);
                function flap() {
                    leftWing.transition([transform:rotate(-80deg),duration:300]).transition([transform:rotate(0deg),duration:300]);
                    rightWing.transition([transform:rotate(80deg),duration:300]).transition([transform:rotate(0deg),duration:300]);
                    body.set([color:red]);
                };
            };
            var myBird = Bird(100px,200px).set([left:500px,top:300px]);
            myBird.flap();
            myBird.flap();
            myBird.flap();
            ";

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(22);
            statements[0].ShouldBeCreateFromClassStatement("myBird", "Bird", typeof(GenericNode), TimeSpan.Zero, new[] { "100px", "200px" });
            statements[1].ShouldBeCreateChildStatement("myBird-body", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-body.png", "100px", "200px" });
            statements[2].ShouldBeSetPropertyStatement("myBird-body", "z-index", "1", TimeSpan.Zero);
            statements[3].ShouldBeCreateChildStatement("myBird-leftWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-left-wing.png", "100px", "200px" });
            statements[4].ShouldBeCreateChildStatement("myBird-rightWing", "myBird", typeof(Image), TimeSpan.Zero, new[] { "assets/flying-bird-right-wing.png", "100px", "200px" });
            statements[5].ShouldBeSetPropertyStatement("myBird", "left", "500px", TimeSpan.Zero);
            statements[6].ShouldBeSetPropertyStatement("myBird", "top", "300px", TimeSpan.Zero);
            statements[7].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(-80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.Zero);
            statements[8].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300));
            statements[9].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.Zero);
            statements[10].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300));
            statements[11].ShouldBeSetPropertyStatement("myBird-body", "color", "red", TimeSpan.Zero);
            statements[12].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(-80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(600));
            statements[13].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(900));
            statements[14].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(600));
            statements[15].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(900));
            statements[16].ShouldBeSetPropertyStatement("myBird-body", "color", "red", TimeSpan.Zero);
            statements[17].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(-80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1200));
            statements[18].ShouldBeSetTransitionStatement("myBird-leftWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1500));
            statements[19].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(80deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1200));
            statements[20].ShouldBeSetTransitionStatement("myBird-rightWing", new Dictionary<string, string> { { "transform", "rotate(0deg)" } }, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1500));
            statements[21].ShouldBeSetPropertyStatement("myBird-body", "color", "red", TimeSpan.Zero);
        }

        //[Test]
        public void Run_ShouldCreateClassNodes_GivenClassDefinition()
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

            var statements = state.SceneCompiler.Compile(script);

            statements.Count.Should().Be(2);
            statements[0].ShouldBeCreateStatement("node", typeof(MyNode), TimeSpan.Zero);
            statements[1].ShouldBeSetPropertyStatement("node", "position", "absolute", TimeSpan.Zero);
        }
    }
}
