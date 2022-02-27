using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor.Graph
{
    public class StoryGraphView : GraphView
    {
        private readonly Vector2 _defaultNodeSize = new Vector2(200, 150);
        public readonly Vector2 defaultCommentBlockSize = new Vector2(300, 200);
        private NodeSearchWindow _searchWindow;

        public StoryGraphView(EditorWindow editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("NarrativeGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            var startNode = CreateStartNode(this);
            if (startNode != null) AddElement(startNode);
        
            AddSearchWindow(editorWindow);
        }

        private void AddSearchWindow(EditorWindow editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Configure(editorWindow, this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public Group CreateCommentBlock(Rect rect, CommentBlockData commentBlockData = null)
        {
            commentBlockData ??= new CommentBlockData();

            var group = new Group
            {
                autoUpdateGeometry = true,
                title = commentBlockData.title
            };
            AddElement(group);
            group.SetPosition(rect);
            return group;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node &&
                    startPort.portColor == port.portColor && startPort.direction != port.direction)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public void CreateNewDialogueNode(string guid, string nodeTitle, string nodeText, Character speaker, Vector2 position)
        {
            AddElement(CreateDialogueNode(guid, nodeTitle, nodeText, speaker, position));
        }
        public void CreateNewValueNode(string guid, ValueData data, Vector2 position)
        {
            AddElement(CreateValueNode(guid, data, position));
        }
        public void CreateNewEndNode(string guid, string nodeName, Vector2 position)
        {
            AddElement(CreateEndNode(guid, nodeName, position));
        }
        public void CreateNewBranchNode(string guid, string nodeName, Vector2 position)
        {
            AddElement(CreateBranchNode(guid, nodeName, position));
        }
        public void CreateNewEventNode(string guid, Vector2 position, ValueData data)
        {
            AddElement(CreateEventNode(guid, position, data));
        }

        public DialogueNode CreateDialogueNode(string guid, string nodeTitle, string nodeText, Character speaker, Vector2 position)
        {
            var tempDialogueNode = new DialogueNode()
            {
                speaker = speaker,
                nodeTitle = nodeTitle,
                dialogueText = nodeText,
                guid = string.IsNullOrEmpty(guid) ? GetUniqueGuid(typeof(DialogueNode)) : guid
            };
        
            tempDialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        
            //TITLE CONTAINER
            //nodeTitle
            tempDialogueNode.titleContainer.Clear();

            var titleField = new TextField("")
            {
                style =
                {
                    height = 30,
                    alignSelf = Align.Center,
                    maxWidth = 500,
                    minWidth = 100
                }
            };
        
            titleField.RegisterValueChangedCallback(evt =>
            {
                tempDialogueNode.nodeTitle = evt.newValue;
            });

            titleField.SetValueWithoutNotify(tempDialogueNode.nodeTitle);
            tempDialogueNode.titleContainer.Add(titleField);
        
            //Character Container
            var objectField = new ObjectField
            {
                objectType = typeof(Character),
                style =
                {
                    height = 20,
                    alignSelf = Align.Center
                }
            };

            objectField.RegisterValueChangedCallback(evt =>
            {
                tempDialogueNode.speaker = (Character) evt.newValue;
            });
            objectField.SetValueWithoutNotify(tempDialogueNode.speaker);
            tempDialogueNode.titleContainer.Add(objectField);
        
            //MAIN CONTAINER
            //Input port
            var inputPort = GetPortInstance(tempDialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            inputPort.portColor = Color.white;
            tempDialogueNode.inputContainer.Add(inputPort);
        
            tempDialogueNode.RefreshExpandedState();
            tempDialogueNode.RefreshPorts();
            tempDialogueNode.SetPosition(new Rect(position, _defaultNodeSize));

            //Dialogue text
            var textField = new TextField("");
            textField.RegisterValueChangedCallback(evt =>
            {
                tempDialogueNode.dialogueText = evt.newValue;
            });
        
            textField.SetValueWithoutNotify(tempDialogueNode.dialogueText);
            tempDialogueNode.mainContainer.Add(textField);
        
            var generatedPort = GetPortInstance(tempDialogueNode, Direction.Output,Port.Capacity.Multi);
            generatedPort.portColor = Color.white;

            generatedPort.portName = "Output";
            tempDialogueNode.outputContainer.Add(generatedPort);
            tempDialogueNode.RefreshExpandedState();
            tempDialogueNode.RefreshPorts();
        
            return tempDialogueNode;
        }
    
        public EventNode CreateEventNode(string guid, Vector2 position, ValueData data)
        {
            var tempEventNode = new EventNode()
            {
                title = "Event",
                data = data,
                guid = string.IsNullOrEmpty(guid) ? GetUniqueGuid(typeof(EventNode)) : guid
            };

            var inputPort = GetPortInstance(tempEventNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            inputPort.portColor = Color.white;
            tempEventNode.inputContainer.Add(inputPort);

            var objectField = new ObjectField { objectType = typeof(ValueData) };

            objectField.RegisterValueChangedCallback(evt => { tempEventNode.data = (ValueData)evt.newValue; });
            objectField.SetValueWithoutNotify(tempEventNode.data);
            tempEventNode.mainContainer.Add(objectField);

            var outputPort = GetPortInstance(tempEventNode, Direction.Output);
            outputPort.portName = "Output";
            outputPort.portColor = Color.white;
            tempEventNode.outputContainer.Add(outputPort);

            tempEventNode.RefreshExpandedState();
            tempEventNode.RefreshPorts();
            tempEventNode.SetPosition(new Rect(position,
                _defaultNodeSize));

            return tempEventNode;
        }
    
        public BranchNode CreateBranchNode(string guid, string nodeName, Vector2 position)
        {
            var tempBranchNode = new BranchNode()
            {
                title = nodeName,
                guid = string.IsNullOrEmpty(guid) ? GetUniqueGuid(typeof(BranchNode)) : guid
            };

            var inputPort = GetPortInstance(tempBranchNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            inputPort.portColor = Color.white;
            tempBranchNode.inputContainer.Add(inputPort);

            var preditcatePort = GetPortInstance(tempBranchNode, Direction.Input);
            preditcatePort.portName = "Predicate";
            preditcatePort.portColor = Color.magenta;
            tempBranchNode.inputContainer.Add(preditcatePort);

            var generatedTruePort = GetPortInstance(tempBranchNode, Direction.Output);
            generatedTruePort.portName = "True";
            generatedTruePort.portColor = Color.white;
            tempBranchNode.outputContainer.Add(generatedTruePort);

            var generatedFalsePort = GetPortInstance(tempBranchNode, Direction.Output);
            generatedFalsePort.portName = "False";
            generatedFalsePort.portColor = Color.white;
            tempBranchNode.outputContainer.Add(generatedFalsePort);


            tempBranchNode.RefreshExpandedState();
            tempBranchNode.RefreshPorts();
            tempBranchNode.SetPosition(new Rect(position,
                _defaultNodeSize));

            return tempBranchNode;
        }
    
        public EndNode CreateEndNode(string guid, string nodeName, Vector2 position)
        {
            var tempEndNode = new EndNode()
            {
                title = nodeName,
                guid = string.IsNullOrEmpty(guid) ? GetUniqueGuid(typeof(EndNode)) : guid
            };

            var inputPort = GetPortInstance(tempEndNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "End";
            inputPort.portColor = Color.white;
            tempEndNode.inputContainer.Add(inputPort);

            tempEndNode.RefreshExpandedState();
            tempEndNode.RefreshPorts();
            tempEndNode.SetPosition(new Rect(position,
                _defaultNodeSize));

            return tempEndNode;
        }
    
        public ValueNode CreateValueNode(string guid, ValueData data, Vector2 position)
        {
            var tempValueNode = new ValueNode()
            {
                data = data,
                title = "Value Node",
                guid = string.IsNullOrEmpty(guid) ? GetUniqueGuid(typeof(ValueNode)) : guid
            };
        

            tempValueNode.titleContainer.Clear();

            var titleLabel = new Label("Value Node")
            {
                style =
                {
                    alignSelf = Align.Center,
                    marginLeft = new StyleLength(StyleKeyword.Auto),
                    marginRight = new StyleLength(StyleKeyword.Auto),
                    fontSize = 12
                }
            };

            tempValueNode.titleContainer.Add(titleLabel);

            tempValueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            var outputPort = GetPortInstance(tempValueNode, Direction.Output, Port.Capacity.Multi);
            outputPort.portName = "Input";
            outputPort.portColor = Color.magenta;
            tempValueNode.outputContainer.Add(outputPort);

            var objectField = new ObjectField
            {
                objectType = typeof(ValueData)
            };

            objectField.RegisterValueChangedCallback(evt =>
            {
                tempValueNode.data = (ValueData) evt.newValue;
            });
            objectField.SetValueWithoutNotify(tempValueNode.data);
            tempValueNode.mainContainer.Add(objectField);

            tempValueNode.RefreshExpandedState();
            tempValueNode.RefreshPorts();
            tempValueNode.SetPosition(new Rect(position,
                _defaultNodeSize));

            return tempValueNode;
        }

        private static Port GetPortInstance(Node node, Direction nodeDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
        }
    
        public BaseNode CreateStartNode(GraphView graphView)
        {
            foreach (var n in graphView.nodes.ToList())
            {
                var node = n as BaseNode;
                if (node.entryPoint)
                {
                    return null;
                }
            }
        
            var nodeCache = new BaseNode()
            {
                title = "Inicio",
                guid = "Inicio",
                entryPoint = true
            };

            var generatedPort = GetPortInstance(nodeCache, Direction.Output, Port.Capacity.Multi);
            generatedPort.portName = "Inicio";
            generatedPort.portColor = Color.white;
            nodeCache.outputContainer.Add(generatedPort);

            nodeCache.capabilities &= ~Capabilities.Movable;
            nodeCache.capabilities &= ~Capabilities.Deletable;

            nodeCache.RefreshExpandedState();
            nodeCache.RefreshPorts();
            nodeCache.SetPosition(new Rect(100, 200, 100, 150));
        
            return nodeCache;
        }

        private string GetUniqueGuid(Type t, bool isEntryPoint = false)
        {
            if (isEntryPoint)
                return "Inicio";
        
            var i = -1;
            string guid;

            bool unique;
            do
            {
                i++;
                unique = true;
                guid = t + "- " + i;
            
            
                foreach (var n in nodes.ToList())
                {
                    if (n.GetType() != t) continue;
                    var nGuid = (n as BaseNode).guid.Replace(t + "- ", "");
                
                    if (nGuid == i.ToString()) unique = false;
                }

            } while (!unique);

            return guid;
        }
    
    }
}