using System;

[Serializable]
public class NodeLinkData
{
    public string baseNodeGuid;
    public string portName;
    public string targetNodeGuid;

    public NodeLinkData(string baseNodeGuid, string portName, string targetNodeGuid)
    {
        this.baseNodeGuid = baseNodeGuid;
        this.portName = portName;
        this.targetNodeGuid = targetNodeGuid;
    }

    public NodeLinkData()
    {

    }
}