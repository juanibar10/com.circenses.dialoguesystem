using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor.Graph
{
    public class StoryGraph : EditorWindow
    {
        private static StoryGraph instance { get; set; }

        public DialogueContainer dialogueFile;
        private StoryGraphView _graphView;
        private Toggle _autoSave;

        [MenuItem("Graph/Dialogue Graph")]
        public static void CreateGraphViewWindow()
        {
            var dialogueSettings = AssetDatabase.LoadAssetAtPath<DialogueSystemSettings>(
                "Packages/com.circenses.dialoguesystem/Editor/DialogueSystemSettings.asset");
            
            if (dialogueSettings)
                GetWindow<StoryGraph>(dialogueSettings.selectedWindow);
            else
                GetWindow<StoryGraph>();
        }

        private void OnEnable()
        {
            instance = this;
        }

        private void ConstructGraphView()
        {
            rootVisualElement.Clear();
            var lastFile = AssetDatabase.LoadAssetAtPath<DialogueSystemSettings>(
                "Packages/com.circenses.dialoguesystem/Editor/DialogueSystemSettings.asset");
            
            dialogueFile = lastFile.lastOpenFile;
            
            EditorUtility.SetDirty(lastFile);
            AssetDatabase.SaveAssetIfDirty(lastFile);

            if (dialogueFile == null)
            {
                Debug.Log("No hay nada");
                return;
            }
            _graphView = new StoryGraphView(this)
            {
                name = dialogueFile.name,
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void RequestDataOperation(bool save, DialogueContainer file, bool clearList = false)
        {
            if (string.IsNullOrEmpty(file.name)) return;
            var saveUtility = GraphSaveUtility.GetInstance(_graphView);

            if (save)
            {
                var lastFile = AssetDatabase.LoadAssetAtPath<DialogueSystemSettings>(
                    "Packages/com.circenses.dialoguesystem/Editor/DialogueSystemSettings.asset");
                lastFile.lastOpenFile = file;
                saveUtility.SaveGraph(file, clearList);
            }
            else
            {
                saveUtility.LoadNarrative(file);
            }
        }

        public void CreateGUI()
        {
            CreateStoryGraph();
        }

        private void CreateStoryGraph()
        {
            ConstructGraphView();

            var dialogueSettings = AssetDatabase.LoadAssetAtPath<DialogueSystemSettings>(
                "Packages/com.circenses.dialoguesystem/Editor/DialogueSystemSettings.asset");
            if (dialogueSettings.lastOpenFile == null) return;

            dialogueFile = dialogueSettings.lastOpenFile;

            RequestDataOperation(false, dialogueFile);
            titleContent.text = dialogueSettings.lastOpenFile.name;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.circenses.dialoguesystem/Scripts/Editor/Resources/Toolbar.uxml");
            VisualElement labelFromUxml = visualTree.Instantiate();
            var toolbar = labelFromUxml.Children();
            var buttons = toolbar.First().Children().ToList();

            var saveButton = (ToolbarButton) buttons[0];
            var clearButton = (ToolbarButton) buttons[1];
        
            _autoSave = (Toggle) buttons[2];
            _autoSave.value = AssetDatabase
                .LoadAssetAtPath<DialogueSystemSettings>(DialogueSystemSettings.DialogueSettingsPath)
                .autoSaveDafault;

            if (saveButton != null)
                saveButton.clicked += () => RequestDataOperation(true, dialogueFile);
            if(clearButton != null)
                clearButton.clicked += () => GraphSaveUtility.GetInstance(_graphView).ClearGraphButton(dialogueFile);

            rootVisualElement.Add(labelFromUxml);
        }
    

        private void Update()
        {
            AutoSave();
        }

        private void AutoSave()
        {
            if (_autoSave == null) return;
            
            if (!_autoSave.value) return;
            if (GrabbingAnyEdge()) return;

            RequestDataOperation(true, dialogueFile, true);
        }

        private bool GrabbingAnyEdge()
        {
            if (_graphView == null) return true;
        
            var edges = _graphView.edges.ToList();
            if (edges.Count < 1) return true;

            foreach (var edge in edges)
            {
                if (edge.input == null) return true;

                if (edge.output == null) return true;
            }

            return false;
        }


        private void OnDisable()
        {
            if (rootVisualElement != null && rootVisualElement.childCount > 0) rootVisualElement.Remove(_graphView);
        }

        [OnOpenAsset(1)]
        public static bool OnOpenDatabase(int instanceID, int line)
        {
            var container = EditorUtility.InstanceIDToObject(instanceID) as DialogueContainer;
            if (container == null) return false;
        
            var lastFile = AssetDatabase.LoadAssetAtPath<DialogueSystemSettings>(
                "Packages/com.circenses.dialoguesystem/Editor/DialogueSystemSettings.asset");
            lastFile.lastOpenFile = container;
            
            EditorUtility.SetDirty(lastFile);
            AssetDatabase.SaveAssetIfDirty(lastFile);
        
            if (instance)
            {
                instance.dialogueFile = container;
                instance.CreateStoryGraph();
            }
            else
            {
                var dialogueSettings = AssetDatabase.LoadAssetAtPath<DialogueSystemSettings>(
                    "Packages/com.circenses.dialoguesystem/Editor/DialogueSystemSettings.asset");
                if (dialogueSettings)
                    GetWindow<StoryGraph>(dialogueSettings.selectedWindow);
                else
                    GetWindow<StoryGraph>();
            }
        
            return true;
        }
    
        public static System.Type[] GetAllEditorWindowTypes()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            var editorWindow = typeof(EditorWindow);
            return (from a in assemblies from T in a.GetTypes() where T.IsSubclassOf(editorWindow) select T).ToArray();
        }
    }
}