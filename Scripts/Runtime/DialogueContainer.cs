using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<DialogueNodeData> dialogueNodeData = new List<DialogueNodeData>();
    public List<EndNodeData> endNodeData = new List<EndNodeData>();
    public List<BranchNodeData> branchNodeData = new List<BranchNodeData>();
    public List<EventNodeData> eventNodeData = new List<EventNodeData>();
    public List<ValueNodeData> valueNodeData = new List<ValueNodeData>();
    public List<CommentBlockData> commentBlockData = new List<CommentBlockData>();


#if UNITY_EDITOR
     [MenuItem("Assets/Create/Dialogue Container",false, 0)]
    private static void CreateDialogueContainer()
    {
        var dialogueContainerObject = Resources.Load<DialogueContainer>("Container");

        var guid = "DialogueNode- " + 0;
        var inicioGuid = "Inicio";
        
        if (dialogueContainerObject == null)
        {
            var asset = CreateInstance<DialogueContainer>();

            
            asset.nodeLinks.Add(new NodeLinkData(inicioGuid, "Inicio",
                guid));

            asset.dialogueNodeData.Add(new DialogueNodeData(guid, "Dialogue Option", "Dialogue Text",
                null, new Vector2(212, 200)));

            AssetDatabase.CreateAsset(asset, "Assets/Resources/Container.asset");
            AssetDatabase.SaveAssets();
        }
        else
        {
            var i = 0;
            do
            {
                i++;
                dialogueContainerObject = Resources.Load<DialogueContainer>($"Container {i}");
            }
            while (dialogueContainerObject != null);

            var asset = CreateInstance<DialogueContainer>();

            asset.nodeLinks.Add(new NodeLinkData(inicioGuid, "Inicio",
                guid));

            asset.dialogueNodeData.Add(new DialogueNodeData(guid, "Dialogue Option", "Dialogue Text",
                null, new Vector2(212, 200)));

            AssetDatabase.CreateAsset(asset, $"Assets/Resources/Container {i}.asset");
            AssetDatabase.SaveAssets();
        }
    }
#endif
    public void ClearLists()
    {
        nodeLinks.Clear();
        dialogueNodeData.Clear();
        endNodeData.Clear();
        branchNodeData.Clear();
        valueNodeData.Clear();
        eventNodeData.Clear();
        commentBlockData.Clear();
    }
}
