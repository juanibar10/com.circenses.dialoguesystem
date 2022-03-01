using System.Collections.Generic;
using System.Linq;
using DialogueSystem.Editor.Graph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Editor
{
    public class GraphSaveUtility
    {
        private IEnumerable<Edge> edges => GetAllEdges();
        private IEnumerable<BaseNode> nodes => AddAllNodes();
        private IEnumerable<DialogueNode> dialogueNodes => AddDialogueNodes();
        private IEnumerable<EndNode> endNodes => AddEndNodes();
        private IEnumerable<BranchNode> branchNodes => AddBranchNodes();
        private IEnumerable<EventNode> eventNodes => AddEventNodes();
        private IEnumerable<ValueNode> valueNodes => AddValueNode();
        private IEnumerable<Group> commentBlocks =>
            _graphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();

        private DialogueContainer _dialogueContainer;
        private StoryGraphView _graphView;

        public static GraphSaveUtility GetInstance(StoryGraphView graphView)
        {
            return new GraphSaveUtility
            {
                _graphView = graphView
            };
        }

        public void SaveGraph(DialogueContainer file, bool clearList)
        {
            var dialogueContainerObject = file;

            if (clearList)
            {
                dialogueContainerObject.ClearLists();
            }
            
            if (dialogueContainerObject is null)
            {
                dialogueContainerObject = ScriptableObject.CreateInstance<DialogueContainer>();
                dialogueContainerObject.ClearLists();
                
                SaveNodes(dialogueContainerObject);
                SaveCommentBlocks(dialogueContainerObject);
            
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");

                AssetDatabase.CreateAsset(dialogueContainerObject, $"Assets/Resources/{file}.asset");
            }
            else
            {
                dialogueContainerObject.ClearLists();

                SaveNodes(dialogueContainerObject);
                SaveCommentBlocks(dialogueContainerObject);
            }
            
            EditorUtility.SetDirty(dialogueContainerObject);
            AssetDatabase.SaveAssetIfDirty(dialogueContainerObject);
        }

        private void SaveNodes(DialogueContainer dialogueContainerObject)
        {
            var connectedSockets = edges.Where(x => x.input.node != null).ToArray();
            var nodeLinks = new List<NodeLinkData>();
            
            foreach (var t in connectedSockets)
            {
                var nodeLink = new NodeLinkData();

                if (t.output == null) continue;
                
                var outputNode = t.output.node as BaseNode;
                nodeLink.baseNodeGuid = outputNode.guid;

                nodeLink.portName = t.output.portName;

                if (t.input == null) continue;
                var inputNode = t.input.node as BaseNode;
                nodeLink.targetNodeGuid = inputNode.guid;

                nodeLinks.Add(nodeLink);
            }

            dialogueContainerObject.nodeLinks = RemoveDuplicated(nodeLinks);

            foreach (var node in dialogueNodes.Where(node => !node.entryPoint))
            {
                dialogueContainerObject.dialogueNodeData.Add(new DialogueNodeData
                {
                    nodeGuid = node.guid,
                    title = node.nodeTitle,
                    dialogueText = node.dialogueText,
                    speaker = node.speaker,
                    position = node.GetPosition().position
                });
            }
            foreach (var node in endNodes.Where(node => !node.entryPoint))
            {
                dialogueContainerObject.endNodeData.Add(new EndNodeData
                {
                    nodeGuid = node.guid,
                    position = node.GetPosition().position
                });
            }
            foreach (var node in branchNodes.Where(node => !node.entryPoint))
            {
                dialogueContainerObject.branchNodeData.Add(new BranchNodeData
                {
                    nodeGuid = node.guid,
                    position = node.GetPosition().position
                });
            }
            foreach (var node in valueNodes.Where(node => !node.entryPoint))
            {
                dialogueContainerObject.valueNodeData.Add(new ValueNodeData()
                {
                    nodeGuid = node.guid,
                    position = node.GetPosition().position,
                    data = node.data
                });
            }
            foreach (var node in eventNodes.Where(node => !node.entryPoint))
            {
                dialogueContainerObject.eventNodeData.Add(new EventNodeData
                {
                    nodeGuid = node.guid,
                    data =  node.data,
                    position = node.GetPosition().position
                });
            }
        }
        private List<NodeLinkData> RemoveDuplicated(List<NodeLinkData> list)
        {
            var links = new List<NodeLinkData>();
            foreach (var link in list)
            {
                var contains = false;
                foreach (var _ in links.Where(l =>
                    link.baseNodeGuid == l.baseNodeGuid && link.portName == l.portName &&
                    link.targetNodeGuid == l.targetNodeGuid)) contains = true;

                if (!contains) links.Add(link);

            }
            return links;
        }
        
        private IEnumerable<Edge> GetAllEdges()
        {
            if (_graphView == null) return new List<Edge>();
            return _graphView.edges.ToList();
        }
        
        private IEnumerable<BaseNode> AddAllNodes()
        {
            var nd = dialogueNodes.Cast<BaseNode>().ToList();
            nd.AddRange(endNodes);
            nd.AddRange(valueNodes);
            nd.AddRange(branchNodes);
            nd.AddRange(eventNodes);
            return nd;
        }
        
        private IEnumerable<DialogueNode> AddDialogueNodes()
        {
            return (from item in _graphView.nodes.ToList()
                where item.GetType() == typeof(DialogueNode)
                select item as DialogueNode).ToList();
        }
        private IEnumerable<EndNode> AddEndNodes()
        {
            return (from item in _graphView.nodes.ToList()
                where item.GetType() == typeof(EndNode)
                select item as EndNode).ToList();
        }
        private IEnumerable<ValueNode> AddValueNode()
        {
            return (from item in _graphView.nodes.ToList()
                where item.GetType() == typeof(ValueNode)
                select item as ValueNode).ToList();
        }
        private IEnumerable<BranchNode> AddBranchNodes()
        {
            return (from item in _graphView.nodes.ToList()
                where item.GetType() == typeof(BranchNode)
                select item as BranchNode).ToList();
        }
        private IEnumerable<EventNode> AddEventNodes()
        {
            return (from item in _graphView.nodes.ToList()
                where item.GetType() == typeof(EventNode)
                select item as EventNode).ToList();
        }

        private void SaveCommentBlocks(DialogueContainer dialogueContainer)
        {
            foreach (var block in commentBlocks)
            {
                var nds = block.containedElements.Where(x => x is DialogueNode).Cast<DialogueNode>().Select(x => x.guid).ToList();
                nds.AddRange(block.containedElements.OfType<BranchNode>().Select(containedElement => containedElement).Select(n => n.guid));
                nds.AddRange(block.containedElements.OfType<EventNode>().Select(containedElement => containedElement).Select(n => n.guid));
                nds.AddRange(block.containedElements.OfType<EndNode>().Select(containedElement => containedElement).Select(n => n.guid));

                dialogueContainer.commentBlockData.Add(new CommentBlockData
                {
                    childNodes = nds,
                    title = block.title,
                    position = block.GetPosition().position
                });
            }
        }
        
        public void LoadNarrative(DialogueContainer file)
        {
            _dialogueContainer = file;
            if (_dialogueContainer == null)
            {
                EditorUtility.DisplayDialog("Archivo no encontrado", "El archivo que buscas no existe", "Continuar");
                return;
            }

            ClearGraph();
            GenerateNodes();
            ConnectNodes();
            GenerateCommentBlocks();
        }

        public void ClearGraphButton(DialogueContainer file)
        {
            _dialogueContainer = file;
            if (_dialogueContainer == null)
            {
                EditorUtility.DisplayDialog("Archivo no encontrado", "El archivo que buscas no existe", "Continuar");
                return;
            }

            ClearGraph();
        }

        private void ClearGraph()
        {
            _graphView.DeleteElements(edges);
            _graphView.DeleteElements(_graphView.nodes.ToList());
            
            var startNode = _graphView.CreateStartNode(_graphView);
            if (startNode != null) _graphView.AddElement(startNode);
        }
        
        private void GenerateNodes()
        {
            var startNode = _graphView.CreateStartNode(_graphView);
            if (startNode != null) _graphView.AddElement(startNode);
            
            foreach (var perNode in _dialogueContainer.branchNodeData)
            {
                var tempNode = _graphView.CreateBranchNode(perNode.nodeGuid, "Branch", perNode.position);
                tempNode.guid = perNode.nodeGuid;
                _graphView.AddElement(tempNode);
            }

            foreach (var perNode in _dialogueContainer.eventNodeData)
            {
                var tempNode = _graphView.CreateEventNode(perNode.nodeGuid, perNode.position, perNode.data);
                tempNode.guid = perNode.nodeGuid;
                tempNode.data = perNode.data;
                _graphView.AddElement(tempNode);
            }

            foreach (var perNode in _dialogueContainer.dialogueNodeData)
            {
                var tempNode = _graphView.CreateDialogueNode(perNode.nodeGuid, perNode.title, perNode.dialogueText, perNode.speaker, perNode.position);
                tempNode.guid = perNode.nodeGuid;
                _graphView.AddElement(tempNode);
            }

            foreach (var perNode in _dialogueContainer.endNodeData)
            {
                var tempNode = _graphView.CreateEndNode(perNode.nodeGuid, "End", perNode.position);
                tempNode.guid = perNode.nodeGuid;
                _graphView.AddElement(tempNode);
            }

            foreach (var perNode in _dialogueContainer.valueNodeData)
            {
                var tempNode = _graphView.CreateValueNode(perNode.nodeGuid, perNode.data, perNode.position);
                tempNode.guid = perNode.nodeGuid;
                _graphView.AddElement(tempNode);
            }
        }

        private void ConnectNodes()
        {
            var startNode = _graphView.nodes.ToList().Select(n => n as BaseNode)
                .FirstOrDefault(node => node.entryPoint);

            if (startNode != null)
            {
                var connections = _dialogueContainer.nodeLinks.Where(x => x.baseNodeGuid == startNode.guid).ToList();

                if (connections.Count > 0)
                {
                    foreach (var link in connections)
                    {
                        var targetNodeGuid = link.targetNodeGuid;

                        foreach (var item in nodes)
                        {
                            if (item.guid != targetNodeGuid) continue;
                            LinkNodesTogether((Port)startNode.outputContainer[0], (Port)item.inputContainer[0]);
                        }
                    }
                }
            }

            foreach (var dialogueNode in dialogueNodes)
            {
                var connections = new List<NodeLinkData>();
                foreach (var link in _dialogueContainer.nodeLinks)
                {
                    if (link.baseNodeGuid == dialogueNode.guid)
                    {
                        connections.Add(link);
                    }
                }

                if (connections.Count > 0)
                {
                    foreach (var link in connections)
                    {
                        var targetGuid = link.targetNodeGuid;

                        foreach (var node in nodes)
                        {
                            if (node.guid == targetGuid)
                            {
                                LinkNodesTogether((Port)dialogueNode.outputContainer[0], (Port)node.inputContainer[0]);
                            }
                        }
                    }
                }
            }

            foreach (var branchNode  in branchNodes)
            {
                var connections = new List<NodeLinkData>();
                foreach (var link in _dialogueContainer.nodeLinks)
                {
                    if (link.baseNodeGuid == branchNode.guid)
                    {
                        connections.Add(link);
                    }
                }

                if (connections.Count > 0)
                {
                    foreach (var link in connections)
                    {
                        var targetGuid = link.targetNodeGuid;

                        foreach (var node in nodes)
                        {
                            if (node.guid == targetGuid)
                            {
                                var output = link.portName == "True" ? 0 : 1;
                                LinkNodesTogether((Port)branchNode.outputContainer[output], (Port)node.inputContainer[0]);
                            }
                        }
                    }
                }
            }

            foreach (var eventNode  in eventNodes)
            {
                var connections = new List<NodeLinkData>();
                foreach (var link in _dialogueContainer.nodeLinks)
                {
                    if (link.baseNodeGuid == eventNode.guid)
                    {
                        connections.Add(link);
                    }
                }

                if (connections.Count > 0)
                {
                    foreach (var link in connections)
                    {
                        var targetGuid = link.targetNodeGuid;

                        foreach (var node in nodes)
                        {
                            if (node.guid == targetGuid)
                            {
                                LinkNodesTogether((Port)eventNode.outputContainer[0], (Port)node.inputContainer[0]);
                            }
                        }
                    }
                }
            }
            
            foreach (var valueNode  in valueNodes)
            {
                var connections = new List<NodeLinkData>();
                foreach (var link in _dialogueContainer.nodeLinks)
                {
                    if (link.baseNodeGuid == valueNode.guid)
                    {
                        connections.Add(link);
                    }
                }

                if (connections.Count > 0)
                {
                    foreach (var link in connections)
                    {
                        var targetGuid = link.targetNodeGuid;

                        foreach (var node in nodes)
                        {
                            if (node.guid == targetGuid)
                            {
                                LinkNodesTogether((Port)valueNode.outputContainer[0], (Port)node.inputContainer[1]);
                            }
                        }
                    }
                }
            }
        }

        private void LinkNodesTogether(Port outputSocket, Port inputSocket)
        {
            if (outputSocket == null) return;
        
            var tempEdge = new Edge()
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            _graphView.Add(tempEdge);
        }
        
        private void GenerateCommentBlocks()
        {
            foreach (var commentBlock in commentBlocks)
            {
                _graphView.RemoveElement(commentBlock);
            }

            foreach (var commentBlockData in _dialogueContainer.commentBlockData)
            {
                var block = _graphView.CreateCommentBlock(
                    new Rect(commentBlockData.position, _graphView.defaultCommentBlockSize),
                    commentBlockData);
                block.AddElements(endNodes.Where(x => commentBlockData.childNodes.Contains(x.guid)));
                block.AddElements(dialogueNodes.Where(x => commentBlockData.childNodes.Contains(x.guid)));
                block.AddElements(branchNodes.Where(x => commentBlockData.childNodes.Contains(x.guid)));
                block.AddElements(eventNodes.Where(x => commentBlockData.childNodes.Contains(x.guid)));
            }
        }
    }
}
