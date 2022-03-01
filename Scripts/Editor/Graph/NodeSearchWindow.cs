using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor.Graph
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow _window;
        private StoryGraphView _graphView;
        private Texture2D _indentationIcon;

        public void Configure(EditorWindow window, StoryGraphView graphView)
        {
            _window = window;
            _graphView = graphView;

            //Transparent 1px indentation icon as a hack
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeEntry(new GUIContent("Value Node",_indentationIcon))
                {
                    level = 1,
                    userData = new ValueNode()
                },
                new SearchTreeEntry(new GUIContent("Dialogue Node", _indentationIcon))
                {
                    level = 1,
                    userData = new DialogueNode()
                },
                new SearchTreeEntry(new GUIContent("Branch", _indentationIcon))
                {
                    level = 1,
                    userData = new BranchNode()
                },
                new SearchTreeEntry(new GUIContent("Event", _indentationIcon))
                {
                    level = 1,
                    userData = new EventNode()
                },
                new SearchTreeEntry(new GUIContent("End Node", _indentationIcon))
                {
                    level = 1,
                    userData = new EndNode()
                },
                new SearchTreeEntry(new GUIContent("Comment Block",_indentationIcon))
                {
                    level = 1,
                    userData = new Group()
                }
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            //Editor window-based mouse position
            var mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);
            switch (searchTreeEntry.userData)
            {
                case DialogueNode _:
                    _graphView.CreateNewDialogueNode("", "Dialogue Option", "Dialogue Text", null, graphMousePosition);
                    return true;
                case BranchNode _:
                    _graphView.CreateNewBranchNode("", "Branch", graphMousePosition);
                    return true;
                case EventNode _:
                    _graphView.CreateNewEventNode("", graphMousePosition, null);
                    return true;
                case EndNode _:
                    _graphView.CreateNewEndNode("", "End", graphMousePosition);
                    return true;
                case ValueNode _:
                    _graphView.CreateNewValueNode("", null, graphMousePosition);
                    return true;
                case Group _:
                    var rect = new Rect(graphMousePosition, _graphView.defaultCommentBlockSize);
                    _graphView.CreateCommentBlock(rect);
                    return true;
            }
            return false;
        }
    }
}
