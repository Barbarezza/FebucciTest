using System;
using UnityEngine;

[Serializable]
public class NodeOutputPort : NodePort
{

    private const float PORT_MARGIN_LEFT_RIGHT = 10;
    
    public NodeOutputPort(Node parentNode) : base(parentNode)
    {
        
    }
    protected override void UpdateRectBasedOnParentNode()
    {
        int countInParentNode = ParentNode.NodeOutputPorts.Count;
        int indexInParentNode = ParentNode.NodeOutputPorts.IndexOf(this);
        
        float x = ParentNode.UsedRect.x + ParentNode.UsedRect.width - PORT_WIDTH - PORT_MARGIN_LEFT_RIGHT;
        
        float spacing = ParentNode.UsedRect.height / (countInParentNode + 1);
        float y = ParentNode.UsedRect.y + spacing * (indexInParentNode + 1) - PORT_HEIGTH/2;
        
        UsedRect = new Rect(x, y, PORT_WIDTH,PORT_HEIGTH);
    }
}
